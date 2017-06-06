require 'json'
require 'optparse'
include Math

# global definitions
@valid_components = ['Lights_EyeBrightness', 'Lights_One', 'Lights_Two', 'Lights_Three', 'Lights_Four', 'Lights_Five', 'Lights_Six', 
                      'Lights_Seven', 'Lights_Eight', 'Lights_Nine', 'Lights_Ten', 'Lights_Eleven', 'Lights_Twelve', 
                      'Lights_ChestRed', 'Lights_ChestGreen', 'Lights_ChestBlue', 
                      'Lights_RightEarRed', 'Lights_RightEarGreen', 'Lights_RightEarBlue', 
                      'Lights_LeftEarRed', 'Lights_LeftEarGreen', 'Lights_LeftEarBlue', 
                      'Lights_Tail', 
                      'Lights_Sound', 
                      'MoveControl_translate.translateX', 'MoveControl_translate.translateY', 'MoveControl_rotate.rotateZ',
                      'Head_rotate.rotateY', 'Head_rotate.rotateZ'
                    ]

@component_eye_ring = 100 # "eye"
@component_rgb_left_ear = 102 # "left_ear_light"
@component_rgb_right_ear = 103 # "right_ear_light"
@component_rgb_chest = 104 # "chest_light"
@component_led_tail = 105 # "tail_light"

@component_motor_servo_head_tilt = 202 # "head_tilt"
@component_motor_servo_head_pan = 203 # "head_pan"

@component_motion_body_linear_angular = 204
@component_motion_body_pose = 205 # body pose

@component_sound = 300

@pi = (3.14159265359)
@two_pi = (@pi * 2)

@pose_frames_presend = 3
@pose_set_num = 2

# pulling these values out to be global since we now need to modify it for sound
@temp_sound_info = nil
@current_component = nil
@current_component_type = nil

# start of method definitions
def should_parse_values?
  if (@current_component_type == "Lights") && (@current_component.include?("ound_"))
    # sound is parsed differently since it was hacked in maya, so need to treat it differently here
    @temp_sound_info = @current_component
    @current_component = "Sound"
  end 
  component_str = "#{@current_component_type}_#{@current_component}"
  result = @valid_components.include?(component_str)
  puts "parsed component: #{component_str}" if result
  result
end

# put the raw data in array, ordered by frame
def raw_data_from_file(filename)
  parsed_data = {}
  parse_value = false
  puts "file to be parsed: #{filename}"
  File.open(filename, "r") do |f|
    f.each_line do |line|
      words = line.split(' ')
      if (Integer(words[0]) rescue nil) && parse_value #parsing the value!
        frame_number = words[0].to_i #array is 0-based
        value = words[1].to_f
        unless @temp_sound_info.nil?
          frame_number = words[1].to_i # value should always be the frame
          value = @temp_sound_info # construct the sound file name
        end

        components_hash = parsed_data[frame_number]
        components_hash ||= {}        
        components_hash["#{@current_component_type}_#{@current_component}"] = value 
        parsed_data[frame_number] = components_hash
      else
        case words[0]
          when "anim" # start of a new value for a given component, remember the results
            if words.size >= 7
              @current_component = words[1]  # component name
              type_str = words[3].split(":")
              if (type_str.size == 1) # component type (Head, MoveControls, Lights)
                @current_component_type = type_str[0]
              else
                @current_component_type = type_str[1]
              end
              parse_value = should_parse_values?
            end
          when "keys {" # after this line, start parsing
            parse_value = true if parse_value 
          when "}" # done parsing, set it to false to ignore lines
            parse_value = false 
            @current_component = nil
            @temp_sound_info = nil # resetting the sound
        end
      end
    end
  end
  parsed_data
end

def set_nested_hash_value(hash, index, nested_hash_index, nested_hash_value)
  nested_hash = hash[index]
  nested_hash[nested_hash_index] = nested_hash_value
  hash[index] = nested_hash
end
def convert_to_light_brightness(value)
  (value / 255.0).round(2) # normalize to 0-1 rather than 0-255
end
def convert_to_eye_light_brightness(value)
  255 # always full on.
end
def convert_to_robot_x(value)
  (value * -1.0).round(2)
end
def convert_to_robot_y(value)
  (value * -1.0).round(2)
end
def convert_to_robot_theta(value)
  value.round(2)
end
def convert_to_sound_file_str(raw_string)
  values = raw_string.split("_")
  dir_name = values[1].upcase
  file_name = values[2..-1].join('_').upcase
  file_str = "#{dir_name}/#{file_name}"
  file_str
end

# iterate through maya frames and convert it into commands
def raw_data_to_commands(raw_data)
  commands = []
  prev_x = nil
  prev_y = nil
  prev_theta_deg = nil
  have_first_pose = false
  
  if @options[:motion] == 'pose'
     cur_frame = @pose_frames_presend+1
  else
    cur_frame = 0
  end
  
  # todo: get this from the anim file.
  duration = 0.0306 # 33ms per frame

  # add empty frames to the begining of the commands array so that we can pre-send pose commands
  # the first one will be used for the coordinate system reset, the rest will be for pre-sending pose commands
  if @options[:motion] == 'pose'
    for i in 1...(@pose_frames_presend + 2)
      command = {"frame" => i, "duration" => duration, "commands" => {}}
      commands << command
    end
  end

  raw_data.sort.map do |key, state|

    cur_frame = cur_frame + 1
    
    pose = {}
    pose_set = {}
    
    components_main = {
        @component_eye_ring => {},
        @component_rgb_left_ear => {},
        @component_rgb_right_ear => {},
        @component_rgb_chest => {},
        @component_led_tail => {},
        @component_motor_servo_head_tilt => {},
        @component_motor_servo_head_pan => {},
        @component_sound => {}
    }

    if @options[:motion] == 'linang'
      components_main[@component_motion_body_linear_angular] = {}
    #elsif @options[:motion] == 'pose'
    #  components_pose[@component_motion_body_pose] = {}
    end

    curr_x = nil
    curr_y = nil
    curr_theta_deg = nil

    eye_component = { "index" => {} }
    state.each_pair do |maya_component_index, maya_component_value|
      case maya_component_index
        when 'MoveControl_translate.translateX'
          curr_x = convert_to_robot_x(maya_component_value)
          #puts "curx #{key} #{curr_x}"
        when 'MoveControl_translate.translateY'
          curr_y = convert_to_robot_y(maya_component_value)
          #puts "cury #{key} #{curr_y}"
        when 'MoveControl_rotate.rotateZ'
          curr_theta_deg = convert_to_robot_theta(maya_component_value)
          #puts "curz #{key} #{curr_theta_deg}"
        when 'Head_rotate.rotateY'
          set_nested_hash_value(components_main, @component_motor_servo_head_tilt, "degree", -1 * maya_component_value.round(2))
        when 'Head_rotate.rotateZ'
          set_nested_hash_value(components_main, @component_motor_servo_head_pan, "degree", maya_component_value.round(2))
        #print(maya_component_value.round(0));
        when 'Lights_ChestRed'
          set_nested_hash_value(components_main, @component_rgb_chest, "r", convert_to_light_brightness(maya_component_value))
        when 'Lights_ChestGreen'
          set_nested_hash_value(components_main, @component_rgb_chest, "g", convert_to_light_brightness(maya_component_value))
        when 'Lights_ChestBlue'
          set_nested_hash_value(components_main, @component_rgb_chest, "b", convert_to_light_brightness(maya_component_value))
        when 'Lights_RightEarRed'
          set_nested_hash_value(components_main, @component_rgb_right_ear, "r", convert_to_light_brightness(maya_component_value))
        when 'Lights_RightEarGreen'
          set_nested_hash_value(components_main, @component_rgb_right_ear, "g", convert_to_light_brightness(maya_component_value))
        when 'Lights_RightEarBlue'
          set_nested_hash_value(components_main, @component_rgb_right_ear, "b", convert_to_light_brightness(maya_component_value))
        when 'Lights_LeftEarRed'
          set_nested_hash_value(components_main, @component_rgb_left_ear, "r", convert_to_light_brightness(maya_component_value))
        when 'Lights_LeftEarGreen'
          set_nested_hash_value(components_main, @component_rgb_left_ear, "g", convert_to_light_brightness(maya_component_value))
        when 'Lights_LeftEarBlue'
          set_nested_hash_value(components_main, @component_rgb_left_ear, "b", convert_to_light_brightness(maya_component_value))
        when 'Lights_Tail'
          set_nested_hash_value(components_main, @component_led_tail, "brightness", convert_to_light_brightness(maya_component_value))
        when 'Lights_EyeBrightness'          
          eye_component["brightness"] = convert_to_eye_light_brightness(maya_component_value)
        when 'Lights_One'
          value = maya_component_value == 1 ? true : false
          set_nested_hash_value(eye_component, "index", "1", value)
        when 'Lights_Two'
          value = maya_component_value == 1 ? true : false
          set_nested_hash_value(eye_component, "index", "2", value)
        when 'Lights_Three'
          value = maya_component_value == 1 ? true : false
          set_nested_hash_value(eye_component, "index", "3", value)
        when 'Lights_Four'
          value = maya_component_value == 1 ? true : false
          set_nested_hash_value(eye_component, "index", "4", value)
        when 'Lights_Five'
          value = maya_component_value == 1 ? true : false
          set_nested_hash_value(eye_component, "index", "5", value)
        when 'Lights_Six'
          value = maya_component_value == 1 ? true : false
          set_nested_hash_value(eye_component, "index", "6", value)
        when 'Lights_Seven'
          value = maya_component_value == 1 ? true : false
          set_nested_hash_value(eye_component, "index", "7", value)
        when 'Lights_Eight'
          value = maya_component_value == 1 ? true : false
          set_nested_hash_value(eye_component, "index", "8", value)
        when 'Lights_Nine'
          value = maya_component_value == 1 ? true : false
          set_nested_hash_value(eye_component, "index", "9", value)
        when 'Lights_Ten'
          value = maya_component_value == 1 ? true : false
          set_nested_hash_value(eye_component, "index", "10", value)
        when 'Lights_Eleven'
          value = maya_component_value == 1 ? true : false
          set_nested_hash_value(eye_component, "index", "11", value)
        when 'Lights_Twelve'
          value = maya_component_value == 1 ? true : false
          set_nested_hash_value(eye_component, "index", "12", value)
        when 'Lights_Sound'
          value = convert_to_sound_file_str(maya_component_value)
          set_nested_hash_value(components_main, @component_sound, "file", value)
          set_nested_hash_value(components_main, @component_sound, "volume", 0.9)
      end
    end

    if !curr_x.nil? && !curr_y.nil? && !curr_theta_deg.nil?
      if @options[:motion] == 'pose'
        #puts "everything #{key} #{curr_x} #{curr_y} #{curr_theta_deg}"
        #set_nested_hash_value(components_pose, @component_motion_body_pose, "x", curr_x)
        #set_nested_hash_value(components_pose, @component_motion_body_pose, "y", curr_y)
        #set_nested_hash_value(components_pose, @component_motion_body_pose, "degree", curr_theta_deg)
        pose["x"] = curr_x
        pose["y"] = curr_y
        pose["degree"] = curr_theta_deg
        pose["mode"] = 5
        pose["dir"] = 2
        pose["time"] = @pose_frames_presend * duration

        if !have_first_pose
          pose_set["x"] = curr_x
          pose_set["y"] = curr_y
          pose_set["degree"] = curr_theta_deg
          pose_set["mode"] = 4
        end
        
      else
        if !prev_x.nil? && !prev_y.nil? && !prev_theta_deg.nil?
          d_x = curr_x - prev_x
          d_y = curr_y - prev_y
          d_theta_deg = curr_theta_deg - prev_theta_deg
          d_theta_rad = d_theta_deg * @two_pi / 360
          dist_straight = sqrt((d_x * d_x) + (d_y * d_y))
          curr_theta_rad = curr_theta_deg * @two_pi / 360

          # detect backwards motion
          heading_x = cos(curr_theta_rad)
          heading_y = sin(curr_theta_rad)
          motion_dot_heading = (heading_x * d_x) + (heading_y * d_y)
          if motion_dot_heading < 0
            dist_straight *= -1
          end

          # we account for the fact that in linear/angular, the robot travels along arcs.
          # the effect is pretty small.
          # basic formula is D = (dTheta * d) / (2 * sin(dTheta / 2)).

          # wrap dTheta into [-PI, PI)
          d_theta_wrapped = d_theta_rad
          while d_theta_wrapped < -@pi
            d_theta_wrapped += @two_pi
          end
          while d_theta_wrapped > @pi
            d_theta_wrapped -= @two_pi
          end

          sin_d_theta_over_two = sin(d_theta_wrapped / 2)

          # method becomes unstable as d_theta approaches 0, but it approaches dist_straight.
          if sin_d_theta_over_two < 0.0001
            dist_arc = dist_straight
          else
            dist_arc = (d_theta_wrapped * dist_straight) / (2 * sin_d_theta_over_two)
          end

          dist_arc_scaled = dist_arc * @options[:scale]

          # set_nested_hash_value(components, @component_motion_body_linear_angular, "debug: curr_theta_deg", curr_theta_deg)
          # set_nested_hash_value(components, @component_motion_body_linear_angular, "debug: d_theta_wrapped", d_theta_wrapped)
          # set_nested_hash_value(components, @component_motion_body_linear_angular, "debug: d_theta_rad", d_theta_rad)
          # set_nested_hash_value(components, @component_motion_body_linear_angular, "debug: sin_d_theta_over_two", sin_d_theta_over_two)
          # set_nested_hash_value(components, @component_motion_body_linear_angular, "debug: dist_straight", dist_straight.round(3))
          set_nested_hash_value(components_main, @component_motion_body_linear_angular, "linear_cm_s", (dist_arc_scaled / duration).round(3))
          set_nested_hash_value(components_main, @component_motion_body_linear_angular, "angular_cm_s", (d_theta_rad / duration).round(3))
        end

        prev_x = curr_x
        prev_y = curr_y
        prev_theta_deg = curr_theta_deg
      end
    end

    components_main[@component_eye_ring] = eye_component unless eye_component["index"].empty?
    components_main.delete_if { |key, values| values.empty? }
    command = {"frame" => cur_frame, "duration" => duration, "commands" => components_main}
    commands << command

    if !have_first_pose && !pose_set.empty?
      for n in 0..(@pose_set_num-1)
        commands[n]["commands"][@component_motion_body_pose] = pose_set
      end
      have_first_pose = true
    end

    frame_offset = @pose_frames_presend + 1 - (@pose_set_num - 1)
    
    if !pose.empty?
      if commands[cur_frame-frame_offset] == nil
        commands[cur_frame-frame_offset] = {"frame" => cur_frame-frame_offset, "duration" => duration, "commands" => {}}
      end
      commands[cur_frame-frame_offset]["commands"][@component_motion_body_pose] = pose
    end
    
  end
  commands
end

def merge_commands_if_necessary(commands)
  merged_commands = []
  first_command = true
  commands.each do |command|
    if first_command
      first_command = false
    end
    merged_commands << command  
  end
  merged_commands
end

def commands_to_file(commands, filename)
  content = { 'data' => merge_commands_if_necessary(commands) }
  File.open(filename, "w") do |f|
    #f.write(content.to_json)
    f.write(JSON.pretty_generate(content))
  end
end

def output_filename_from_input_filename(input_filename)
  if File.extname(input_filename).downcase == ".anim"
    output_str = File.dirname(input_filename) + "/" + File.basename(input_filename, ".anim") + ".json"
  else
  output_str = input_filename.clone
    output_str = output_str + ".json"
  end

  # return the same file name with .json extension, in the same directory
#  output_str = input_filename.clone

#  output_str = output_str[/\/\w+\.\w+/] # parses the fully qualified name 
  #output_str = "#{output_str[1..-6]}.json" 

  
  output_str
end

def parse_cmd_options
  options = {}
  OptionParser.new do |opts|
    opts.banner = "Usage: maya_parser.rb [options]"
    options[:input] = nil
    opts.on( '-i', '--input FILE', 'Location of the maya format file to parse' ) do |file|
      options[:input] = file
    end

    options[:directory] = nil
    opts.on( '-d', '--directory DIR', 'Scan through this directory and convert all .anim files' ) do |dir|
      options[:directory] = dir
    end

    options[:motion] = 'linang'
    opts.on( '-m', '--motion [LINANG | pose]', 'whether to interpret body motion as linear/angular or pose' ) do |motion|
      options[:motion] = motion.downcase
    end

    options[:scale] = 1
    opts.on( '-s', '--scale FACTOR', 'LINANAG only: how much to scale the x,y coordinates by. default = 1' ) do |scale|
      options[:scale] = scale.to_f
    end

  end.parse!

  raise OptionParser::MissingArgument if options[:directory].nil? && options[:input].nil?
  raise OptionParser::InvalidArgument if options[:motion] != 'linang' && options[:motion] != 'pose'

  options
end

# start of code execution!
@options = parse_cmd_options()
@files_to_be_parsed = []
if @options[:directory]
  dir_str = @options[:directory]
  Dir.foreach(dir_str) do |file_name|
    @files_to_be_parsed << "#{dir_str}/#{file_name}" if file_name.include?(".anim") #insert fully qualified file name
  end
else
  @files_to_be_parsed << @options[:input]
end

@files_to_be_parsed.each do |input_file|
  raw_states = raw_data_from_file(input_file)
  tmp = raw_states.sort
  #puts "raw states: #{tmp}"
  commands = raw_data_to_commands(raw_states)
  #puts "commands: #{commands}"
  output_file = output_filename_from_input_filename(input_file)
  puts "parsed frames: #{raw_states.size}, converted commands: #{commands.size}, generated output to: #{output_file}"
  commands_to_file(commands, output_file)
end



# @left_wheel_prevX = @left_wheel_prevY = @left_wheel_prevZ = 0.0
# @right_wheel_prevX = @right_wheel_prevY = @right_wheel_prevZ = 0.0
# @left_wheel_currentX = @left_wheel_currentY = @left_wheel_currentZ = 0.0
# @right_wheel_currentX = @right_wheel_currentY = @right_wheel_currentZ = 0.0

# def degree_to_radian(degree)
#   degree * PI / 180
# end

# def radian_to_degree(radian)
#   radian * 180 / PI
# end

# def rotation_angle_delta(z1, z2)
#   delta = z2 - z1
#   # adjust the bounds as necessary (i.e. 359->0 should be 1 degree delta, not -359)
#   # Assumption: robot will not turn >180 within 1 frame (33ms)
#   delta = delta - 360 if delta > 180
#   delta = delta + 360 if delta < -180
#   delta
# end

# @i = 0
# # method to calculate the wheel speed from maya coordinates
# def set_wheel_component_translate(components, left_x1, left_y1, right_x1, right_y1, left_x2, left_y2, right_x2, right_y2, left_z1, right_z1, duration_seconds)
#   speed_multiplier = 1.75
#   left_direct_distance = sqrt((left_x2 - left_x1)**2 + (left_y2 - left_y1)**2)
#   right_direct_distance = sqrt((right_x2 - right_x1)**2 + (right_y2 - right_y1)**2)

#   left_motion_direction = 90 - radian_to_degree(atan2(left_x2 - left_x1, left_y2 - left_y1))
#   left_dot_angle = (left_motion_direction - left_z1)

#   right_motion_direction = 90 - radian_to_degree(atan2(right_x2 - right_x1, right_y2 - right_y1))
#   right_dot_angle = (right_motion_direction - right_z1)

#   if(left_dot_angle > 180)
#     left_dot_angle = left_dot_angle - 360
#   end

#   if(left_dot_angle < -180)
#     left_dot_angle = left_dot_angle + 360
#   end

#   if(right_dot_angle > 180)
#     right_dot_angle = right_dot_angle - 360
#   end

#   if(right_dot_angle < -180)
#     right_dot_angle = right_dot_angle + 360
#   end

#   if(left_dot_angle >= -90 && left_dot_angle <= 90)
#     left_wheel_velocity = -speed_multiplier * left_direct_distance / duration_seconds
#   else
#     left_wheel_velocity = speed_multiplier * left_direct_distance / duration_seconds
#   end

#   if(right_dot_angle >= -90 && right_dot_angle <= 90)
#     right_wheel_velocity = -speed_multiplier * right_direct_distance / duration_seconds
#   else
#     right_wheel_velocity = speed_multiplier * right_direct_distance / duration_seconds
#   end

#   # velocity is expressed in cm/s, converting units as such and rounding it to 100th mm accuracy
#   set_nested_hash_value(components, @component_motor_wheel_right, "velocity", right_wheel_velocity.round(3))
#   set_nested_hash_value(components, @component_motor_wheel_left, "velocity", left_wheel_velocity.round(3))

#   #print(@i)
#   @i=@i+1
# end


# def convert_to_cartesian_left(axis, value)
#   case axis.downcase
#     when 'y'
#       @left_wheel_prevY = @left_wheel_currentY
#       @left_wheel_currentY = value
#     when 'x'
#       @left_wheel_prevX = @left_wheel_currentX
#       @left_wheel_currentX = value
#     when 'z'
#       @left_wheel_prevZ = @left_wheel_currentZ
#       @left_wheel_currentZ = value % 360
#   end
# end

# def convert_to_cartesian_right(axis, value)
#   case axis.downcase
#     when 'y'
#       @right_wheel_prevY = @right_wheel_currentY
#       @right_wheel_currentY = value
#     when 'x'
#       @right_wheel_prevX = @right_wheel_currentX
#       @right_wheel_currentX = value
#     when 'z'
#       @right_wheel_prevZ = @right_wheel_currentZ
#       @right_wheel_currentZ = value % 360
#   end
# end
