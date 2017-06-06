#ifndef ROBOT_HW_H
#define ROBOT_HW_H

#include "HALCommon.h"

#include "Bot2CtlrMsg.h"
#include "Ctlr2BotMsg.h"

namespace HAL {
    class RobotHW
    {
	public:
        static void cleanupHWData(void *);
        
        // Methods for Ctlr2BotMsg
		static void *initComponentsForCtlr2BotMsg(Ctlr2BotMsg *msg, HALBotDescription_t *robotdesc);
		static std::vector<HALPacket_t *>packetizeCtlr2BotMsg(Ctlr2BotMsg *msg, HALBotDescription_t *robotdesc, void *hw_data);
        static void unPacketizeCtlr2BotMsg(Ctlr2BotMsg *msg, HALBotDescription_t *robotdesc, void* hw_data, const void *packet, size_t length);
        static void setCommand(HALBotDescription_t *robotdesc, void *hw_data, PIComponentId id);

        // Methods for Bot2CtrlMsg
        static void *initComponentsForBot2CtlrMsg(Bot2CtlrMsg *msg, HALBotDescription_t *robotdesc);
		static std::vector<HALPacket_t *>packetizeBot2CtlrMsg(Bot2CtlrMsg *msg, HALBotDescription_t *robotdesc, void *hw_data);
        static void unPacketizeBot2CtlrMsg(Bot2CtlrMsg *msg, HALBotDescription_t *robotdesc, void* hw_data, const void *packet, size_t length);
        static void setSensor(HALBotDescription_t *robotdesc, void *hw_data, PIComponentId id);
        
	private:
    };
}

#endif /* ROBOT_HW_H */
