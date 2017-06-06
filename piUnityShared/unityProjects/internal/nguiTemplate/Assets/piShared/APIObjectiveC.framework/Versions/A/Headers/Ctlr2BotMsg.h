#ifdef __cplusplus

#ifndef CTLR2BOTMSG_H
#define CTLR2BOTMSG_H

#include "HALCommon.h"
#include "HALPacket.h"
#include "HALComponentTypes.h"
#include "HALRobotTypes.h"
#include <map>
#include <vector>

namespace HAL {
    class Ctlr2BotMsg
    {
        friend class RobotHW;
        friend class RobotHW_rev0;
	public:
		Ctlr2BotMsg(HALBotDescription_t *robot);
		~Ctlr2BotMsg();
		
		// Packetize takes command components and serializes them to proper packets.
        std::vector<HALPacket_t *>packetize(void);
        // Unpacketize takes a packet and converts it to command components
        void unPacketize(const void *packet, size_t length);
        
		// Accessors for each component type.
		// If a component with this ID doesn't exist for this robot, it will return NULL.
		HALSpeaker_t *getSpeakerStorage(PIComponentId id);
		HALMotorWheel_t *getMotorWheelStorage(PIComponentId id);
		HALMotorServo_t *getMotorServoStorage(PIComponentId id);
		HALLED_t *getLEDStorage(PIComponentId id);
		HALRGB_t *getRGBStorage(PIComponentId id);
		HALEyeRing_t *getEyeRingStorage(PIComponentId id);
        HALPower_t *getPowerStorage(PIComponentId id);
        
        // Called AFTER filling in the component storage
        void setCommand(PIComponentId id);
        
        // Use only for debugging
        void printCommands(void);
        
        std::map<PIComponentId, HALSpeaker_t *> speakers;
        std::map<PIComponentId, HALMotorWheel_t *> wheels;
        std::map<PIComponentId, HALMotorServo_t *> servos;
        std::map<PIComponentId, HALLED_t *> leds;
        std::map<PIComponentId, HALRGB_t *> rgbs;
        std::map<PIComponentId, HALEyeRing_t *> eyeRings;
        std::map<PIComponentId, HALPower_t *> powers;

	private:
		void *_hw_data;
		struct PrivateData;               // Not defined here
    	PrivateData* _internal;
    };
}

#endif /* CTLR2BOTMSG_H */

#endif /* __cplusplus */
