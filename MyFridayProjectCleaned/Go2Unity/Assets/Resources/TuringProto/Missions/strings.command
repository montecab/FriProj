#!/bin/sh
find . -name "*.json" -print0 | xargs -0 -I{}  ../../../../NotForDistribution/Tools/osx/jsonValues.py {} | sort | uniq | tr ' ' '\n' | sed -e 's/<\/*[BbIi]>//g' -e 's/^-*[0123456789].*$//' -e 's/[\!\?\.\"\'\)]$// | sort | uniq
