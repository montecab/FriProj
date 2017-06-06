#ifndef HALCOMPONENTTYPES_H
#define HALCOMPONENTTYPES_H

#include "HALCommon.h"
#include "PIDefinitions.h"

typedef struct {
	PIComponentId id;
    bool valid;
} HALComponentMetadata_t;

typedef struct {
	HALComponentMetadata_t metadata;
	PIButtonState state;
} HALButton_t;

typedef struct {
	HALComponentMetadata_t metadata;
	PISoundEventIndex eventIndex;
	PIVolume volume;
	PIAngle direction;
} HALSoundSensor_t;

typedef struct {
	HALComponentMetadata_t metadata;
	PISoundTrackIndex soundTrackIndex;
	PIVolume volume;
	PIUInteger loops;
} HALSpeaker_t;

typedef struct {
	HALComponentMetadata_t metadata;
	double x;
	double y;
	double z;
} HALIMU3Axis_t;

typedef struct {
	PICentimeter distance;
	PICentimeter margin;
} HALDistanceReading_t;

typedef struct {
	HALComponentMetadata_t metadata;
	HALDistanceReading_t reading;
} HALDistance_t;

typedef struct {
	HALComponentMetadata_t metadata;
	HALDistanceReading_t primaryReading;
	HALDistanceReading_t secondaryReading;
} HALDistancePair_t;

typedef struct {
	HALComponentMetadata_t metadata;
	PIVelocity velocity;
	PICentimeter encoderDistance;
} HALMotorWheel_t;

typedef struct {
	HALComponentMetadata_t metadata;
	PIAngularVelocity velocity;
	PIAngle angle;
} HALMotorServo_t;

typedef struct {
	HALComponentMetadata_t metadata;
	PIBrightness brightness;
} HALLED_t;

typedef struct {
	HALComponentMetadata_t metadata;
	PIBrightness r;
	PIBrightness g;
	PIBrightness b;
} HALRGB_t;

typedef struct {
	HALComponentMetadata_t metadata;
	PIEyeAnimationIndex animationID;
	PIBrightness brightness;
	PIUInteger animationSpeed;
	PIUInteger loops;
	uint16_t bitmap; // Only used when animationID is set to EYEANIM_BITMAP
} HALEyeRing_t;

typedef struct {
    HALComponentMetadata_t metadata;
    PIPowerState powerState;
} HALPower_t;

// HALC_ROBOTDETECT_T
// TODO


typedef struct {
	int32_t min;
	int32_t max;
} PIBotRange32_t;

typedef struct {
	uint32_t min;
	uint32_t max;
} PIBotRange32U_t;

/* TODO: do this range definition with component enums instead of a static struct */
typedef struct {
	PIBotRange32U_t dist_range;
	PIBotRange32_t accel_range;
	PIBotRange32_t gyro_range;
	PIBotRange32_t pan_range;
	PIBotRange32_t tilt_range;
	PIBotRange32U_t encoder_range;
} PIBotSensorRanges_t;

#endif /* HALCOMPONENTTYPES_H */
