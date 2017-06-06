#ifndef DFUPACKET_H
#define DFUPACKET_H

typedef enum
{
    DFU_TARGET_OPCODE_START_FILE = 1,
    DFU_TARGET_OPCODE_INITIALIZE_DFU_PARAMS,
    DFU_TARGET_OPCODE_RECEIVE_FIRMWARE_IMAGE,
    DFU_TARGET_OPCODE_VALIDATE_FIRMWARE,
    DFU_TARGET_OPCODE_ACTIVATE_RESET,
    DFU_TARGET_OPCODE_RESET,
    DFU_TARGET_OPCODE_REPORT_SIZE,
    DFU_TARGET_OPCODE_REQUEST_RECEIPT,
    DFU_TARGET_OPCODE_RESPONSE_CODE = 0x10,
    DFU_TARGET_OPCODE_RECEIPT,
} DFUTargetOpcode;

typedef enum
{
    DFU_TARGET_RESPONSE_SUCCESS = 0x01,
    DFU_TARGET_OPCODE_INVALID_STATE,
    DFU_TARGET_OPCODE_NOT_SUPPORTED,
    DFU_TARGET_OPCODE_DATA_SIZE_EXCEEDS_LIMIT,
    DFU_TARGET_OPCODE_CRC_ERROR,
    DFU_TARGET_OPCODE_OPERATION_FAILED,
    DFU_TARGET_OPCODE_BAD_ADDRESS,
} DFUTargetResponse;

typedef struct __attribute__((packed))
{
    uint8_t opcode;
    union
    {
        uint16_t n_packets;
        struct __attribute__((packed))
        {
            uint8_t   original;
            uint8_t   response;
        };
        uint32_t n_bytes;
    };
} dfu_control_point_data_t;

typedef struct __attribute__((packed))
{
    uint32_t address;
    uint32_t length;
    uint16_t encoding;//plaintext
    uint16_t csumType;//none
    
} dfu_control_point_init_file_t;

typedef enum
{
    FILE_TRANS_ENCODING_NONE
} fileTransEncodings;

typedef enum
{
    FILE_TRANS_CHECKSUM_NONE,
    FILE_TRANS_CHECKSUM_FLETCHER16
} fileTransChecksums;

typedef struct __attribute__((packed))
{
    uint8_t name[16];
    
} dfu_control_point_file_name_t;

enum {
    FILE_DEST_DFU = 'd',
    FILE_DEST_NRF = 'f',
    FILE_DEST_NVT1 = 'v',
};

#endif