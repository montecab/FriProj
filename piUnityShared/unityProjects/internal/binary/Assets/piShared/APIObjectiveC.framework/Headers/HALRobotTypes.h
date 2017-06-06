#ifndef HALROBOTTYPES_H
#define HALROBOTTYPES_H

#ifdef __cplusplus
extern "C" {
#endif
    
typedef struct {
	uint16_t    major;
	uint16_t    minor;
	uint16_t    commits_past_version;
	uint8_t     checkin[12];
	uint32_t    userhash;
	uint8_t     filehash[16];
} PIBotFirmwareDescription_t;

typedef struct {
	uint16_t    robotType;
	uint16_t    robotVersion;
	uint16_t    protocolVersion;
	PIBotFirmwareDescription_t firmware;
} HALBotDescription_t;

#ifdef __cplusplus
}
#endif

#endif /* HALROBOTTYPES_H */
