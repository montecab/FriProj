#ifndef HALCOMMON_H
#define HALCOMMON_H

#include <stddef.h>
#include <stdint.h>
#include "PIDefinitions.h"

#define BYTEATINDEX(input, index) (((uint8_t *)input)[index])

#define LITTLEU16TOHOST(input) ((uint16_t) (BYTEATINDEX(input, 0) | (BYTEATINDEX(input, 1) << 8)))
#define LITTLE16TOHOST(input) ((int16_t) LITTLEU16TOHOST(input))

#endif /* HALCOMMON_H */
