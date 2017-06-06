#!/bin/bash

# script to convert old json animation files w/ human-readable component names to new ones w/ component numbers.
# output is into "fixed/".

files=()

files+=("bo_happy_wiggleSmile.json")
files+=("bo_happy_lookRightMoveRight.json")
files+=("bo_happy_lookLeftMoveLeft.json")
files+=("bo_happy_idle_withGesture.json")
files+=("bo_happy_celebrateCircle.json")
files+=("bo_encouraging_loopIt_start.json")
files+=("bo_eager_promptChild.json")
files+=("bo_confident_strut.json")
files+=("bo_confident_slalom.json")
files+=("bo_cautious_turnAround.json")
files+=("bo_cautious_ohNo.json")
files+=("bo_cautious_moveFwd_unsure.json")
files+=("bo_cautious_lookAround.json")
files+=("bo_cautious_backUpShakeHead_no.json")

for f in ${files[@]}
do
	cat "$f" | sed -e 's/left_ear_light/102/g' -e 's/right_ear_light/103/g' -e 's/chest_light/104/g' -e 's/tail_light/105/g' -e 's/left_wheel/300/g' -e 's/right_wheel/301/g' -e 's/head_tilt/302/g' -e 's/head_pan/303/g' -e 's/eye/100/g' > "fixed/$f"
done
