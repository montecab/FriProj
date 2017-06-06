#ifdef __cplusplus

#ifndef BOTSENSOREVT_H
#define BOTSENSOREVT_H

#include "HALComponentTypes.h"
#include "HALRobotTypes.h"
#include <vector>

namespace HAL {
    class BotSensorEvt
    {
	public:
        friend class Robot_hw;
        
        BotSensorEvt();
		~BotSensorEvt();
		
        std::vector<HALButton_t *> buttonValues;
        std::vector<HALSoundSensor_t *> soundSensorValues;
        std::vector<HALIMU3Axis_t *> imuValues;
        std::vector<HALDistance_t *> distanceValues;
        std::vector<HALDistancePair_t *> distancePairValues;
        std::vector<HALMotorWheel_t *> motorWheelValues;
        std::vector<HALMotorServo_t *> motorServoValues;
        
	private:
    };
}

#endif /* BOTSENSOREVT_H */

#endif /* __cplusplus */
