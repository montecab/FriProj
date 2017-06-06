#ifndef HALPACKET_H
#define HALPACKET_H

#define MAX_PIBOT_PACKET_SIZE 20

#ifdef __cplusplus
extern "C" {
#endif
    
typedef struct {
	uint32_t length;
	uint8_t buffer[MAX_PIBOT_PACKET_SIZE];
} HALPacket_t;
    
#ifdef __cplusplus
}
#endif

#endif /* HALPACKET_H */
