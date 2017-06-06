#ifdef __cplusplus

#ifndef BOT2CTLRMSG_H
#define BOT2CTLRMSG_H

#include "HALCommon.h"
#include "HALPacket.h"
#include "HALComponentTypes.h"
#include "BotSensorEvt.h"
#include "HALRobotTypes.h"
#include <vector>
#include <map>

namespace HAL {
    
    class Bot2CtlrMsg
    {
        friend class RobotHW;
        friend class RobotHW_rev0;

	public:
        
        Bot2CtlrMsg(HALBotDescription_t *robot);
		~Bot2CtlrMsg();
                
        // Packetize takes sensor components and serializes them to proper packets.
        std::vector<HALPacket_t *>packetize(void);
        // Unpacketize takes a packet and converts it to sensor components
        void unPacketize(const void *packet, size_t length);
        
		// Accessors for each component type.
		// If a component with this ID doesn't exist for this robot, it will return NULL.
        HALIMU3Axis_t *getAccelStorage(PIComponentId id);
        HALIMU3Axis_t *getGyroStorage(PIComponentId id);
		HALMotorWheel_t *getMotorWheelStorage(PIComponentId id);
        HALButton_t *getButtonStorage(PIComponentId id);
        HALDistance_t *getDistanceSensorStorage(PIComponentId id);
        
        // Called AFTER filling in the sensor storage
        void setSensor(PIComponentId id);
        
        // Called to print after unpacketizing a sensor message
        void printCommands(void);
        
        std::map<PIComponentId, HALIMU3Axis_t *> accels;
        std::map<PIComponentId, HALIMU3Axis_t *> gyros;
        std::map<PIComponentId, HALMotorWheel_t *> wheels;
        std::map<PIComponentId, HALButton_t *> buttons;
        std::map<PIComponentId, HALDistance_t *> distances;
        
	private:
		void *_hw_data;
		struct PrivateData;               // Not defined here
    	PrivateData* _internal;
    };
}

#endif /* BOT2CTLRMSG_H */

#endif /* __cplusplus */
