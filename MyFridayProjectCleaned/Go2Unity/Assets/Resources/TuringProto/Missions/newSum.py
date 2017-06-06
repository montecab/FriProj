#!/usr/bin/python

import json
import sys
import os
import os.path
import re
import operator
import StringIO
import csv
import datetime
from collections import OrderedDict
import subprocess

class Constants:
	DEFAULT_MASTER_FILE  = "trMissionListInfo.json"
	DEFAULT_REWARDS_FILE = "../trRewardsList.json"
	MODE_NORMAL          = "normal"
	MODE_TOCSV           = "to_csv"
	MODE_FROMCSV         = "from_csv"

class TokensCSV:
	CHALLENGE            = "challenge"
	STEP                 = "step"
	HINT                 = "hint"
	YES                  = "yes"
	HEADER_CHAL_ID       = "challenge id\n(read-only)"
	HEADER_CHAL_NAME     = "challenge name\n(read-only)"
	HEADER_STEP_NUM      = "step #\n(read-only)"
	HEADER_HINT_NUM      = "hint #\n(read-only)"
	HEADER_ITEM_TYPE     = "item type\n(read-only)"
	HEADER_READY         = "rdy 2 xl8"
	HEADER_NAME          = "name"
	HEADER_NUM_CHARS     = "# chars -->"
	HEADER_INTRO         = "intro"
	HEADER_DESC          = "description"
	CSV_ROW_HEADERS      = (
		HEADER_CHAL_ID  ,   # A
		HEADER_CHAL_NAME,   # B
		HEADER_STEP_NUM ,   # C
		HEADER_HINT_NUM ,   # D
		HEADER_ITEM_TYPE,   # E
		HEADER_READY    ,   # F
		HEADER_NAME     ,   # G
		HEADER_NUM_CHARS,   # H
		HEADER_INTRO    ,   # I
		HEADER_NUM_CHARS,   # J
		HEADER_DESC         # K
		)


class Globals:
	run_mode                = Constants.MODE_NORMAL
	verbose                 = False
	master_file_name        = Constants.DEFAULT_MASTER_FILE
	rewards_file_name       = Constants.DEFAULT_REWARDS_FILE
	strings_csv_file_name   = None
	strings_csv_file        = None
	po_file_name            = None
	summary_csv_file_name   = None
	summary_csv_file        = None
	behaviors               = {}
	missions                = {}
	states                  = {}
	start_state             = None
	word_counts             = {}
	csv_rows_written        = {}          # number of spreadsheet rows writte, per file.
	rewards                 = []
	reward_prev             = None
	phrases                 = []			# todo - consider something more performant like a set.
	phrases_for_translation = []
	updated_challenge_ids   = set()
	updated_strings         = 0
	items_became_ready      = 0
	items_became_unready    = 0
	ignore_ready            = False
	list_missions           = False

class Tokens:
	BEHAVIOR_ID        = "behavior_id"
	BEHAVIORS          = "behaviors"
	CHILDREN           = "children"
	COMMENTS           = "comments"
	DESCRIPTION        = "description"
	DURABLES           = "durables"
	FILE_NAME          = "file_name"
	FULL_MISSION       = "full_mission"
	HINTS              = "hints"
	ID                 = "id"
	INTRODUCTION       = "introduction"
	IQ_POINTS          = "iq_points"
	MIN_POINTS         = "min_points"
	MISSION            = "mission"
	MISSIONS           = "missions"
	PARENTS            = "parents"
	PAYLOAD            = "payload"
	PROGRAM            = "program"
	PUZZLES            = "puzzles"
	READY_TO_TRANSLATE = "ready_to_translate"
	ROBOT_TYPE         = "robot_type"
	START_STATE        = "START_STATE"
	STATES             = "states"
	STATE_SOURCE_ID    = "state_source_id"
	STATE_TARGET_ID    = "state_target_id"
	TARGET             = "target"
	TRANSITIONS        = "transitions"
	TYPE               = "type"
	UNKNOWN            = "UNKNOWN"
#	UUID               = "UUID"
	USER_FACING_NAME   = "user_facing_name"
	VALUE              = "value"
	VISITED            = "visited"
	WORD_COUNT         = "word_count"



def usage():
	print "Extracts all sorts of interesting and useful things from challenges."
	print "Can also put useful things back into challenges."
	print
	print "USAGE:"
	print "%s OPTIONS:" % (sys.argv[0])
	print
	print "  --help "
	print "    prints this message"
	print
	print "  --missions rootMissionFile.json"
	print "    specify the input root mission file (default = %s)" % Constants.DEFAULT_MASTER_FILE
	print
	print "  --rewards rewardsFile.json"
	print "    specify the input rewards definition file. (default = %s)" % Constants.DEFAULT_REWARDS_FILE
	print
	print "  --tostringscsv file.csv"
	print "    extract mission copy into the specified .CSV file. this is non-destructive to missions."
	print
	print "  --fromstringscsv file.csv"
	print "    replace mission copy with text from the specified .CSV file. this is destructive to missions and advanced."
	print
	print "  --topo gettextFile.po"
	print "    extract mission copy into the specified gettext (.po) file. this is non-destructive to missions."
	print
	print "  --ignore_ready"
	print "    for use with --topo: exports all strings, ready or not."
	print
	print "  --tosummarycsv summary.csv"
	print "    print mission summary information into the specified .csv file"
	print
	print "  --list_missions"
	print "    just list the mission files"

def check_args():

	local_args = list(sys.argv)

	local_args.pop(0)

	while len(local_args) > 0:
		arg = consume_one_arg(local_args, None)
		if False:
			pass

		elif arg == "--help":
			usage()
			exit(0)
			
		elif arg == "--missions":
			Globals.master_file_name = consume_one_arg(local_args, arg)
			
		elif arg == "--rewards":
			Globals.rewards_file_name = consume_one_arg(local_args, arg)
			
		elif arg == "--tostringscsv":
			if Globals.run_mode != Constants.MODE_NORMAL:
				usage()
				print "\nconflicting arguments: %s" % arg
				exit(1)

			Globals.run_mode              = Constants.MODE_TOCSV
			Globals.strings_csv_file_name = consume_one_arg(local_args, arg)

		elif arg == "--fromstringscsv":
			if Globals.run_mode != Constants.MODE_NORMAL:
				usage()
				print "\nconflicting arguments: %s" % arg
				exit(1)

			Globals.run_mode              = Constants.MODE_FROMCSV
			Globals.strings_csv_file_name = consume_one_arg(local_args, arg)
			
		elif arg == "--topo":
			Globals.po_file_name = consume_one_arg(local_args, arg)
			
		elif arg == "--ignore_ready":
			Globals.ignore_ready = True

		elif arg == "--list_missions":
			Globals.list_missions = True

		elif arg == "--tosummarycsv":
			Globals.summary_csv_file_name = consume_one_arg(local_args, arg)

		else:
			usage()
			print "\nUnrecognized argument: %s" % arg
			exit(1)


def consume_one_arg(args, arg):
	if len(args) == 0:
		usage()
		print "\nMissing parameter for %s" % arg
		exit(1)
	else:
		return args.pop(0)


def main():
	check_args()
	process_rewards()
	process_master_file()

def csvOut_base(fp, fields):
	output = StringIO.StringIO()
	wr = csv.writer(output, quoting=csv.QUOTE_ALL)

	# deal with u\2018 and u\2019, because the csv writer is ASCII only.

	new_fields = ()

	for field in fields:
		
		s = ""
		if isinstance(field, basestring):
			s = field.encode('utf-8')
		else:
			s = str(field)

		s = s.replace(u'\u2018'.encode('utf-8'), "'")
		s = s.replace(u'\u2019'.encode('utf-8'), "'")

		new_fields = new_fields + (s,)

	try:
		wr.writerow(new_fields)
	except:
		print "error serializing fields! {0}".format(fields)
		exit(1)

	fp.write(output.getvalue())

	if fp not in Globals.csv_rows_written:
		Globals.csv_rows_written[fp] = 0
	Globals.csv_rows_written[fp] += 1



def csvOut(fp, headers_and_vals):

	tmp = zip(*headers_and_vals)
	headers = tmp[0]
	values  = tmp[1]

	if not fp in Globals.csv_rows_written:
		csvOut_base(fp, headers)

	csvOut_base(fp, values)

def csvRowNum(fp):
	if not fp in Globals.csv_rows_written:
		print "error: file not begun!"
		return 0
	return Globals.csv_rows_written[fp] + 1


def master_file_name():
	return Globals.master_file_name

def parse_json(file_path):
	fp = open(file_path, "r")
	if not fp:
		print "Error: could not open for read: \"%s\"" % (file_path)
		exit(1)

	file_contents = fp.read()
	fp.close()

	try:
		js_obj = json.loads(file_contents, object_pairs_hook=OrderedDict)
	except:
		print "Error: could not parse \"%s\"" % (file_path)
		exit(1)

	return js_obj

def write_json(js_obj, file_path):
	fp = open(file_path, "w")
	if not fp:
		print "Error: could not open for write: \"%s\"" % (file_path)
		exit(1)

	try:
		json.dump(js_obj, fp, sort_keys=False, indent=3, separators=(', ', ' : '))
	except:
		print "error: could not encode json. \"%s\"" % file_path
		exit(1)

	fp.close()

def process_master_file():
	js_file     = parse_json(master_file_name())
	js_missions = js_file[Tokens.MISSIONS]
	js_program  = js_file[Tokens.PROGRAM]

	process_missions   (js_missions)
	process_behaviors  (js_program)
	process_states     (js_program)
	process_transitions(js_program)

	try_from_csv()

	print_recursive(Globals.start_state, 0, 0)
	print "word-total: %d unique: %d" % (word_count_recursive(Globals.start_state), len(Globals.word_counts))
	print "unique phrases: %d" % (len(Globals.phrases))
	# sorted_words = sorted(Globals.word_counts.items(), key=operator.itemgetter(1))
	# for pair in sorted_words:
	# 	print "%s, %d" % (pair[0].encode('utf-8'), pair[1])

	output_translation_files()


def process_missions(js_missions):
	print "processing %d missions.." % (len(js_missions))
	for entry in js_missions:
		mis_id = entry[Tokens.ID]
		mis    = entry[Tokens.MISSION]
		Globals.missions[mis_id] = mis

def process_behaviors(js_program):
	behaviors = js_program[Tokens.BEHAVIORS]
	print "processing %d behaviors.." % (len(behaviors))
	for entry in behaviors:
		Globals.behaviors[entry[Tokens.ID]] = entry

def process_states(js_program):
	states = js_program[Tokens.STATES]
	print "processing %d states.." % (len(states))
	for entry in states:
		b_id      = entry[Tokens.BEHAVIOR_ID]
		behavior  = Globals.behaviors[b_id]
		if behavior[Tokens.TYPE] == Tokens.START_STATE:
			Globals.start_state = entry
		else:
			mis_id                   = behavior[Tokens.MISSION]
			mis                      = Globals.missions[mis_id]
			mis[Tokens.FILE_NAME]    = mis[Tokens.USER_FACING_NAME] + ".json"
			mis[Tokens.FULL_MISSION] = parse_json(mis[Tokens.FILE_NAME])
			mis[Tokens.ROBOT_TYPE]   = robot_type_for_full_mission(mis[Tokens.FULL_MISSION])

			entry[Tokens.MISSION] = mis

		entry[Tokens.TYPE    ] = behavior[Tokens.TYPE]
		entry[Tokens.PARENTS ] = []
		entry[Tokens.CHILDREN] = []
		entry[Tokens.VISITED ] = False
		Globals.states[entry[Tokens.ID]] = entry

def robot_type_for_full_mission(full_mis):
	rt = None
	for puzzle in full_mis[Tokens.PUZZLES]:
		for hint in puzzle[Tokens.HINTS]:
			prg     = hint[Tokens.PROGRAM]
			hint_rt = prg [Tokens.ROBOT_TYPE]
			if rt == None:
				rt = hint_rt
			elif rt == Tokens.UNKNOWN:
				pass
			else:
				if rt != hint_rt:
					if Globals.verbose:
						print "error: robot type inconsistent in %s " % (full_mis[Tokens.USER_FACING_NAME])
					rt = Tokens.UNKNOWN

	if rt == 1001:
		rt = "Dash"
	elif rt == 1002:
		rt = "Dot"
	elif rt == Tokens.UNKNOWN:
		rt = "????"
	else:
		pass

	return rt



def process_transitions(js_program):
	transitions = js_program[Tokens.TRANSITIONS]
	print "processing %d transitions.." % (len(transitions))
	for entry in transitions:
		st_src_id = entry[Tokens.STATE_SOURCE_ID]
		st_trg_id = entry[Tokens.STATE_TARGET_ID]
		st_src = Globals.states[st_src_id]
		st_trg = Globals.states[st_trg_id]
		st_src[Tokens.CHILDREN].append(st_trg)
		st_trg[Tokens.PARENTS ].append(st_src)

def print_recursive(state, depth, previous_cum_bq):
	if state == None:
		print "ERROR: bad state"

	if state[Tokens.VISITED]:
		print "ERROR: state already visited: %s " % (state[Tokens.MISSION][Tokens.USER_FACING_NAME])
		return

	cumulative_bq = previous_cum_bq

	is_start = (state[Tokens.TYPE] == Tokens.START_STATE)

	if is_start:
		# print "(Start)"
		pass
	else:
		mis = state[Tokens.MISSION]
		cumulative_bq += print_mission(mis, depth, previous_cum_bq)
		if Globals.list_missions:
			print "mission: %s" % mis[Tokens.USER_FACING_NAME]


	if Globals.verbose:
		print "parents : %2d  children: %2d" % (len(state[Tokens.PARENTS ]), len(state[Tokens.CHILDREN]))


	state[Tokens.VISITED] = True

	for child in state[Tokens.CHILDREN]:
		print_recursive(child, depth + 1, cumulative_bq)

def print_mission(mis, depth, previous_cum_bq):
	full_mis = mis[Tokens.FULL_MISSION]
	local_bq = 0
	vid_count = 0
	i_pz = 1
	num_steps = 0
	num_hints = 0
	for puzzle in full_mis[Tokens.PUZZLES]:
		i_hn = 1
		for hint in puzzle[Tokens.HINTS]:
			if Tokens.FILE_NAME in hint:
				video_hint = hint[Tokens.FILE_NAME]
				if (video_hint != "") and (Globals.verbose):
					print "video in \"%s\" (depth %d, %s) step %d hint %d: \"%s\"" % (mis[Tokens.USER_FACING_NAME], depth, mis[Tokens.ROBOT_TYPE], i_pz, i_hn, video_hint)
					vid_count += 1
			i_hn += 1
			num_hints += 1
		local_bq += puzzle[Tokens.IQ_POINTS]
		i_pz += 1
		num_steps += 1

	stored_bq = mis[Tokens.IQ_POINTS]
	if stored_bq != local_bq:
		print "error: in \"%s\", bq points from steps (%d) != bq points in master list (%d)" % (mis[Tokens.USER_FACING_NAME], local_bq, stored_bq)

	cumulative_bq = previous_cum_bq + local_bq

	if Globals.run_mode == Constants.MODE_TOCSV:
		output_strings_mission(full_mis)

	mis[Tokens.WORD_COUNT] = word_count_mission(full_mis)

	reward         = get_highest_reward_for_bq(cumulative_bq)

	if reward == Globals.reward_prev:
		reward = None
	else:
		Globals.reward_prev = reward

	reward_summary = ""
	if reward:
		reward_summary = reward[Tokens.ID] + ": "
		if reward[Tokens.PAYLOAD] != "":
			reward_summary += " " + reward[Tokens.PAYLOAD]
		for dur in reward[Tokens.DURABLES]:
			reward_summary += " " + dur[Tokens.PAYLOAD]


	things = (
		("name"       , mis[Tokens.USER_FACING_NAME]),
		("id"         , mis[Tokens.ID]),
		("depth"      , depth),
		("# steps"    , num_steps),
		("# hints"    , num_hints),
		("robot"      , mis[Tokens.ROBOT_TYPE]),
		("video count", vid_count),
		("local bq"   , local_bq),
		("total bq"   , cumulative_bq),
		("word count" , mis[Tokens.WORD_COUNT]),
		("reward"     , reward_summary)
		)

	if Globals.summary_csv_file_name:
		if not Globals.summary_csv_file:
			try:
				Globals.summary_csv_file = open(Globals.summary_csv_file_name, 'w')
			except:
				print "error: could not open file for writing: \"%s\"" % Globals.summary_csv_file_name
				exit(1)

		csvOut(Globals.summary_csv_file, things)


	return local_bq

def word_count_string(s):
	ns = normalize_string(s)
	if ns == "":
		return 0

	a = ns.split(" ")
	for w in a:
		if w not in Globals.word_counts:
			Globals.word_counts[w] = 0
		Globals.word_counts[w] += 1
	return len(a)

def normalize_string(s):
	result = s

	result = result.upper()

	# newlines to spaces
	result = result.replace("\n", " ")
	result = result.replace("\t", " ")


	# kill b tags
	result = result.replace("<B>", " ")
	result = result.replace("</B>", " ")
	result = result.replace(".", " ")
	result = result.replace("?", " ")
	result = result.replace("!", " ")
	result = result.replace(",", " ")
	result = result.replace("\"", " ")
	result = result.replace("(", " ")
	result = result.replace(")", " ")
	result = result.replace("0", " ")
	result = result.replace("1", " ")
	result = result.replace("2", " ")
	result = result.replace("3", " ")
	result = result.replace("4", " ")
	result = result.replace("5", " ")
	result = result.replace("6", " ")
	result = result.replace("7", " ")
	result = result.replace("8", " ")
	result = result.replace("9", " ")
	result = result.replace("-", " ")

	# collapse whitespace
	result = ' '.join(result.split())



	result = result.strip()
	return result

def word_count_recursive(state):
	if state == None:
		return 0

	children_val = 0

	for child in state[Tokens.CHILDREN]:
		children_val += word_count_recursive(child)

	me_val = 0

	is_start = (state[Tokens.TYPE] == Tokens.START_STATE)

	if not is_start:
		mis = state[Tokens.MISSION]
		me_val = mis[Tokens.WORD_COUNT]

	return me_val + children_val

		
def word_count_mission(full_mis):
	sum = 0

	dsc_c = "#. Challenge \"%s\"" % (full_mis[Tokens.USER_FACING_NAME])

	item_dsc = "%s - Name" % (dsc_c)
	sum += log_phrase(full_mis[Tokens.USER_FACING_NAME], ready_to_translate(full_mis), [item_dsc])
	item_dsc = "%s - Description" % (dsc_c)
	sum += log_phrase(full_mis[Tokens.DESCRIPTION     ], ready_to_translate(full_mis), [item_dsc])

	for puzzle in full_mis[Tokens.PUZZLES]:
		step_num = full_mis[Tokens.PUZZLES].index(puzzle) + 1
		dsc_s = "%s, step %d" % (dsc_c, step_num)
		# item_dsc = "%s - Name" % (dsc_s)
		# sum += log_phrase(puzzle[Tokens.USER_FACING_NAME], ready_to_translate(puzzle), [item_dsc])
		item_dsc = "%s - Introduction" % (dsc_s)
		sum += log_phrase(puzzle[Tokens.INTRODUCTION    ], ready_to_translate(puzzle), [item_dsc])
		item_dsc = "%s - Description" % (dsc_s)
		sum += log_phrase(puzzle[Tokens.DESCRIPTION     ], ready_to_translate(puzzle), [item_dsc])
		for hint in puzzle[Tokens.HINTS]:
			hint_num = puzzle[Tokens.HINTS].index(hint)
			dsc_h = "%s, hint %d" % (dsc_s, hint_num)
			item_dsc = "%s - Description" % (dsc_h)
			sum += log_phrase(hint[Tokens.DESCRIPTION], ready_to_translate(hint), [item_dsc])

	return sum

def output_strings_mission(full_mis):
	if not Globals.strings_csv_file:
		try:
			Globals.strings_csv_file = open(Globals.strings_csv_file_name, 'w')
		except:
			print "error: count not open strings file for writing: \"%s\"", Globals.strings_csv_file_name;
			exit(1)
		csvOut_base(Globals.strings_csv_file, TokensCSV.CSV_ROW_HEADERS)

	row_num = csvRowNum(Globals.strings_csv_file)


	csvOut_base(Globals.strings_csv_file, (
		full_mis[Tokens.ID              ],      # A
		full_mis[Tokens.USER_FACING_NAME],      # B
		"",                                     # C
		"",                                     # D
		TokensCSV.CHALLENGE,                    # E
		ready_to_translate(full_mis),           # F
		"",                                     # G
		"",                                     # H
		"",                                     # I
		"=LEN(K%d)" % row_num,       	        # J
		full_mis[Tokens.DESCRIPTION]            # K
		))

	step_num = 1

	for puzzle in full_mis[Tokens.PUZZLES]:
		row_num = csvRowNum(Globals.strings_csv_file)
		csvOut_base(Globals.strings_csv_file, (
			full_mis[Tokens.ID              ],      # A
			full_mis[Tokens.USER_FACING_NAME],      # B
			step_num,                               # C
			"",                                     # D
			TokensCSV.STEP,                         # E
			ready_to_translate(puzzle),             # F
			puzzle[Tokens.USER_FACING_NAME],        # G
			"=LEN(I%d)" % row_num,       	        # H
			puzzle[Tokens.INTRODUCTION],            # I
			"=LEN(K%d)" % row_num,       	        # J
			puzzle[Tokens.DESCRIPTION]              # K
			))

		hint_num = 1
		for hint in puzzle[Tokens.HINTS]:
			row_num = csvRowNum(Globals.strings_csv_file)
			csvOut_base(Globals.strings_csv_file, (
				full_mis[Tokens.ID              ],    # A
				full_mis[Tokens.USER_FACING_NAME],    # B
				step_num,                             # C
				hint_num,                             # D
				TokensCSV.HINT,                       # E
				ready_to_translate(hint),             # F
				"",                                   # G
				"",                                   # H
				"",                                   # I
				"=LEN(K%d)" % row_num,                # J
				hint[Tokens.DESCRIPTION]              # K
				))

			hint_num += 1


		step_num += 1


def ready_to_translate(item):
	if Tokens.READY_TO_TRANSLATE in item:
		if item[Tokens.READY_TO_TRANSLATE]:
			return TokensCSV.YES
	return ""


def process_rewards():
	Globals.rewards = parse_json(Globals.rewards_file_name)

def get_reward_min_points(reward):
	return reward[Tokens.MIN_POINTS]

def get_rewards_for_bq(bq):
	ret = []
	for reward in Globals.rewards:
		if reward[Tokens.MIN_POINTS] <= bq:
			ret.append(reward)

	ret = sorted(ret, key=get_reward_min_points)

	return ret

def get_highest_reward_for_bq(bq):
	tmp = get_rewards_for_bq(bq)
	if len(tmp) > 0:
		return tmp[-1]
	else:
		return None

def log_phrase(phrase, ready_to_translate, comments):
	if phrase.strip() == '':
		return 0

	phrase_info = {}
	phrase_info[Tokens.VALUE]              = phrase
	phrase_info[Tokens.READY_TO_TRANSLATE] = ready_to_translate
	phrase_info[Tokens.COMMENTS]           = comments
	Globals.phrases_for_translation.append(phrase_info)

	if phrase not in Globals.phrases:
		Globals.phrases.append(phrase)

	return word_count_string(phrase)

def output_translation_files():
	if Globals.po_file_name == None:
		return

	fp = None
	try:
		fp = open(Globals.po_file_name, 'w')
	except:
		print "Error creating PO file: \"%s\"" % Globals.po_file_name
		exit(1)

	output_translation_header(fp)

	ready = 0
	for phrase_obj in Globals.phrases_for_translation:
		ready += output_translation_phrase(fp, phrase_obj)

	fp.close()

	pct = int(100.0 * ready / (float(len(Globals.phrases_for_translation))))

	print "wrote to %s. %d strings available, %d ready for translation. (%d%%)" % (
		Globals.po_file_name, len(Globals.phrases_for_translation), ready, pct)

def git_branch():
	return subprocess.check_output(['git', 'rev-parse', '--abbrev-ref', 'HEAD']).strip()
def git_hash():
	return subprocess.check_output(['git', 'rev-parse', 'HEAD']).strip()
def git_has_local_changes():
	output = subprocess.check_output(['git', 'diff-index', 'HEAD', '--', '.']).strip()
	return output != ""

def output_translation_header(fp):
	local_changes = ''
	if git_has_local_changes():
		local_changes = ' with local changes'
	else:
		local_changes = ' without local changes'

	fp.write('#. note this content is static and therefore has only one form, but we use the standard english Plural-Forms line anyhow to simplify runtime ingestion.\n')
	fp.write('#. from mission files.\n')
	# fp.write('#. git branch:%s hash:%s%s\n' % (git_branch(), git_hash(), local_changes))
	# fp.write('#. time ')
	# fp.write('{:%Y-%m-%d %H:%M:%S}\n'.format(datetime.datetime.now()))
	fp.write('# smartling.placeholder_format_custom = \{[0-9]+\}\n')
	fp.write('msgid ""\n')
	fp.write('msgstr ""\n')
	fp.write('"Content-Type: text/plain; charset=UTF-8\\n"\n')
	fp.write('"Content-Transfer-Encoding: 8bit\\n"\n')
	fp.write('"Plural-Forms: nplurals=2; plural=n != 1;\\n"')
	fp.write('\n')

# see http://www.polarhome.com:813/doc/gettext/gettext_2.html#SEC7
def output_translation_phrase(fp, phrase_info):
	if (not Globals.ignore_ready) and (not phrase_info[Tokens.READY_TO_TRANSLATE]):
		return 0

	phrase = phrase_info[Tokens.VALUE]

	fp.write('\n')

	e_p = encode_phrase(phrase)
	if e_p != '':
		for comment in phrase_info[Tokens.COMMENTS]:
			fp.write("%s\n" % comment)

		fp.write('msgid \"%s\"\n'  % (e_p))
		fp.write('msgstr \"%s\"\n' % (e_p))
	else:
		print "error for phrase: \"%s\" - %s" % (phrase, phrase_info[Tokens.COMMENTS])
		exit(1)

	return 1

def encode_phrase(phrase):
	ret = phrase
	# ret = ret.replace("\\", "\\\\")
	ret = ret.replace("\n", "\\n")
	ret = ret.strip()
	ret = ret.replace("\"", "\\\"")
	return ret.encode("utf-8")

# parse specified CSV file and insert the resulting strings into the original json files (and internal structures)
def try_from_csv():
	if Globals.run_mode != Constants.MODE_FROMCSV:
		return

	print "processing CSV: \"%s\".." % Globals.strings_csv_file_name

	with open(Globals.strings_csv_file_name, 'rb') as fp:
		reader = csv.reader(fp)
		row_num = 0

		column_indices = {}
		curr_challenge = None
		curr_step      = None
		curr_hint      = None

		for row in reader:
			row_num += 1

			new_row = []

			for cell in row:
				cell = cell.replace('\r\n', '\n')
				new_row.append(cell)

			if row_num == 1:
				verify_csv_columns(new_row)
				n = 0
				while n < len(new_row):
					column_indices[new_row[n]] = n
					n += 1

			else:
				challenge_id   = get_named_row(new_row, column_indices, TokensCSV.HEADER_CHAL_ID  )
				challenge_name = get_named_row(new_row, column_indices, TokensCSV.HEADER_CHAL_NAME)
				item_type      = get_named_row(new_row, column_indices, TokensCSV.HEADER_ITEM_TYPE)

				# check for special type challenge here.
				# we do this here so that we can put the consistency-checks here instead of duplicated in Step and Hint.
				if item_type == TokensCSV.CHALLENGE:
					if not challenge_id in Globals.missions:
						print "error: unknown challenge. id: %s name: \"%s\"" % (challenge_id, challenge_name)
						exit(1)
					curr_challenge = Globals.missions[challenge_id][Tokens.FULL_MISSION]
					curr_step      = None
					curr_hint      = None

				else:
					if not curr_challenge:
						print "error: no current challenge. item: %s row: %d csv: %s" % (item_type, row_num, Globals.strings_csv_file_name)
						exit(1)
					if curr_challenge[Tokens.ID] != challenge_id:
						print "error: mismatched challenge. item: %s row: %d csv: %s" % (item_type, row_num, Globals.strings_csv_file_name)
						exit(1)


				csv_name  = get_named_row(new_row, column_indices, TokensCSV.HEADER_NAME)
				csv_intro = get_named_row(new_row, column_indices, TokensCSV.HEADER_INTRO)
				csv_desc  = get_named_row(new_row, column_indices, TokensCSV.HEADER_DESC)
				csv_ready = False

				if get_named_row(new_row, column_indices, TokensCSV.HEADER_READY) == TokensCSV.YES:
					csv_ready = True


				if False:
					pass

				elif item_type == TokensCSV.CHALLENGE:
					# description

					try_update_text(challenge_id, challenge_name, None, None, curr_challenge, Tokens.DESCRIPTION, csv_desc)
					try_update_ready(challenge_id, curr_challenge, csv_ready)

				elif item_type == TokensCSV.STEP:
					# name, intro, and description

					# book-keeping
					curr_hint = None
					step_index = 0
					if curr_step:
						# find next step index in the challenge
						prev_step_index = curr_challenge[Tokens.PUZZLES].index(curr_step)
						step_index = prev_step_index + 1
					step_num = step_index + 1
					if step_index >= len(curr_challenge[Tokens.PUZZLES]):
						print "error: too many steps in challenge. chal: %s step: %d row: %d csv: %s" % (
							challenge_name, step_num, row_num, Globals.strings_csv_file_name)
						exit(1)

					curr_step = curr_challenge[Tokens.PUZZLES][step_index]

					try_update_text(challenge_id, challenge_name, step_num, None, curr_step, Tokens.USER_FACING_NAME, csv_name )
					try_update_text(challenge_id, challenge_name, step_num, None, curr_step, Tokens.INTRODUCTION    , csv_intro)
					try_update_text(challenge_id, challenge_name, step_num, None, curr_step, Tokens.DESCRIPTION     , csv_desc )
					try_update_ready(challenge_id, curr_step, csv_ready)


				elif item_type == TokensCSV.HINT:
					# description

					# book-keeping
					hint_index = 0
					if curr_hint:
						# find next hint index in the step
						prev_hint_index = curr_step[Tokens.HINTS].index(curr_hint)
						hint_index = prev_hint_index + 1
					hint_num = hint_index + 1
					if hint_index >= len(curr_step[Tokens.HINTS]):
						step_index = curr_challenge[Tokens.PUZZLES].index(curr_step)
						print "error: too many hints in step. chal: %s step: %d hint: %d row: %d csv: %s" % (
							challenge_name, step_index + 1, hint_num, row_num, Globals.strings_csv_file_name)
						exit(1)

					curr_hint = curr_step[Tokens.HINTS][hint_index]

					try_update_text(challenge_id, challenge_name, step_num, hint_num, curr_hint, Tokens.DESCRIPTION, csv_desc)
					try_update_ready(challenge_id, curr_hint, csv_ready)

				else:
					print "error: unrecognized item type: %s. challenge: \"%s\" row: %d csv:\"%s\"" % (
						item_type, challenge_name, row_num, Globals.strings_csv_file_name)

	save_challenges(Globals.updated_challenge_ids)

	print ".. finished. %d strings and %d challenges were modified. %d items became ready, %d items are no longer ready." % (
		Globals.updated_strings, len(Globals.updated_challenge_ids), Globals.items_became_ready, Globals.items_became_unready)

def save_challenges(challenge_ids):
	for chal_id in challenge_ids:
		chal_info = Globals.missions[chal_id]
		chal_full = chal_info[Tokens.FULL_MISSION]
		chal_fn   = chal_info[Tokens.FILE_NAME]
		print "writing to challenge: %s - %s" % (chal_id, chal_fn)
		write_json(chal_full, chal_fn)


def try_update_text(challenge_id, challenge_name, step_num, hint_num, obj, key, new_val):
	if (obj[key] != new_val):

		obj[key] = new_val
		Globals.updated_strings += 1
		Globals.updated_challenge_ids.add(challenge_id)

		if Globals.verbose and (hint_num is not None):
			print "updating text: %20s, step %2d, hint %2d, %18s: \"%s\" --> \"%s\"" % (
				challenge_name, step_num, hint_num, key, obj[key], new_val)

		elif Globals.verbose and (step_num is not None):
			print "updating text: %20s, step %2d,          %18s: \"%s\" --> \"%s\"" % (
				challenge_name, step_num, key, obj[key], new_val)

		elif Globals.verbose:
			print "updating text: %20s,                   %18s: \"%s\" --> \"%s\"" % (
				challenge_name, key, obj[key], new_val)

def try_update_ready(challenge_id, obj, new_ready):
	cur_ready = False;

	if Tokens.READY_TO_TRANSLATE in obj:
		cur_ready = obj[Tokens.READY_TO_TRANSLATE]

	if cur_ready != new_ready:
		obj[Tokens.READY_TO_TRANSLATE] = new_ready

		Globals.updated_challenge_ids.add(challenge_id)

		if new_ready:
			Globals.items_became_ready += 1
		else:
			Globals.items_became_unready += 1




def get_named_row(row, column_indices, column_name):
	return row[column_indices[column_name]]

def verify_csv_columns(header_row):
	# verify that each column header we're expecting is somewhere in here, altho not necessarily in the order we expect
	for col in TokensCSV.CSV_ROW_HEADERS:
		if not col in header_row:
			print "error: missing column: \"%s\". csv file: \"%s\" headers: %s" % (col, Globals.strings_csv_file_name, header_row)
			exit(1)


main()


