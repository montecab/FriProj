#ifndef ROBOT_HW0_H
#define ROBOT_HW0_H

#include "HALCommon.h"
#include "RobotHW.h"
#include <vector>

#ifdef __cplusplus
extern "C" {
#endif
    
#define MAX_SEND_PACKETS_rev0 3
    
    // hardware version
#define ROBOT_HW0 0
    
    //7.62cm diameter (7.62cm*pi circumference), 90 slots per rotation
#define PROTOWHEELCMFROMENCODER(encoder) ((encoder/90.0)*M_PI*7.62)
    
#define PROTOGRAVITYFROMACCEL(accel) (accel*120.0/(0x7fff))
    
#define PROTODISTCMFROMSENSOR(sensor) ((double)sensor)
    
#define BRIGHTNESSTOBYTE(brightness) ((uint8_t)((brightness>=0xff)?0xff:brightness))
    
    //prototype speed is 1 hundredth of a millimeter per 30 milliseconds
#define PROTOWHEELSPEEDFROMCMPERSEC(cmpersec) ((int16_t)((cmpersec/100.0)/(.01/1000/(.03))))
    
#define PROTOWHEELSCMPERSECFROMSPEED(speed) ((double)((speed*100.0)*(.01/1000/(.03))))
    
    struct robot_hw0_sensor_data {
        HALIMU3Axis_t accel;
        HALIMU3Axis_t gyro;
        HALMotorWheel_t wheelLeftSensor;
        HALMotorWheel_t wheelRightSensor;
        HALDistance_t distanceLeft;
        HALDistance_t distanceRight;
        HALDistance_t distanceTail;
        HALButton_t buttonMain;
        HALButton_t button1;
        HALButton_t button2;
        HALButton_t button3;
        HALPacket_t packetStorage[MAX_SEND_PACKETS_rev0];
        std::vector<HALPacket_t *>packetized;
    };
    
    struct robot_hw0_cmd_data {
        uint32_t commandMask;
        HALEyeRing_t eyeRing;
        HALRGB_t rgbFront;
        HALRGB_t rgbLeftEar;
        HALRGB_t rgbRightEar;
        HALLED_t ledTail;
        HALLED_t ledButtonMain;
        HALMotorWheel_t wheelLeft;
        HALMotorWheel_t wheelRight;
        HALMotorServo_t headPan;
        HALMotorServo_t headTilt;
        HALSpeaker_t speaker;
        HALPower_t power;
        HALPacket_t packetStorage[MAX_SEND_PACKETS_rev0];
        std::vector<HALPacket_t *>packetized;
    };
    
    /*struct robot_hw0_data {
        uint32_t commandMask;
        HALEyeRing_t eyeRing;
        HALRGB_t rgbFront;
        HALRGB_t rgbLeftEar;
        HALRGB_t rgbRightEar;
        HALLED_t ledTail;
        HALLED_t ledButtonMain;
        HALMotorWheel_t wheelLeft;
        HALMotorWheel_t wheelRight;
        HALMotorServo_t headPan;
        HALMotorServo_t headTilt;
        HALSpeaker_t speaker;
        HALPower_t power;

        HALIMU3Axis_t accel;
        HALMotorWheel_t wheelLeftSensor;
        HALMotorWheel_t wheelRightSensor;
        HALDistance_t distanceLeft;
        HALDistance_t distanceRight;
        HALDistance_t distanceTail;
        HALButton_t buttonMain;
        HALButton_t button1;
        HALButton_t button2;
        HALButton_t button3;
        
        HALPacket_t packetStorage[MAX_SEND_PACKETS_rev0];
        std::vector<HALPacket_t *>packetized;
    };*/
    
#ifdef __cplusplus
}

namespace HAL {
    class RobotHW_rev0
    {
	public:
        // Methods for Ctlr2BotMsg
        static void *initComponentsForCtlr2BotMsg(Ctlr2BotMsg *msg, HALBotDescription_t *robotdesc);
		static std::vector<HALPacket_t *>packetizeCtlr2BotMsg(Ctlr2BotMsg *msg, HALBotDescription_t *robotdesc, void *hw_data);
        static void unPacketizeCtlr2BotMsg(Ctlr2BotMsg *msg, HALBotDescription_t *robotdesc, void* hw_data, const void *packet, size_t length);
        static void setCommand(HALBotDescription_t *robotdesc, void *hw_data, PIComponentId id);
        
        // Methods for Bot2CtlrMsg
        static void *initComponentsForBot2CtlrMsg(Bot2CtlrMsg *msg, HALBotDescription_t *robotdesc);
		static std::vector<HALPacket_t *>packetizeBot2CtlrMsg(Bot2CtlrMsg *msg, HALBotDescription_t *robotdesc, void *hw_data);
        static void unPacketizeBot2CtlrMsg(Bot2CtlrMsg *msg, HALBotDescription_t *robotdesc, void* hw_data, const void *packet, size_t length);
        static void setSensor(HALBotDescription_t *robotdesc, void *hw_data, PIComponentId id);
        
	private:
        static int handleCommand(struct robot_hw0_cmd_data * hw_data, char *buffer, int index);
        
    };
}
#endif

#endif /* ROBOT_HW0_H */
