#!/usr/bin/python

# utility to take a given base file name and a base ID find each of the mood-specific .json files and convert them to .an files.

import sys
import os.path
import subprocess

#######################################################################################

def usage():
	print "%s [-h | --help] [baseTemplate.json id]" % sys.argv[0]
	print "  for example"
	print "  %s runFast<MOOD>.json 1000" % sys.argv[0]

def tryConvert(candidateFileSource, candidateFileOutput):
	if not os.path.isfile(candidateFileSource):
		return 0

	rtype = "-dash"
	if "dash" in candidateFileSource.lower():
		rtype = "-dash"
	elif "dot" in candidateFileSource.lower():
		rtype = "-dot"


	print "converting %s --> %s%s as %s" % (candidateFileSource, candidateFileOutput, rtype, rtype)

	callArgs = []
	callArgs.append(json2robotPath)
	callArgs.append(rtype)
	callArgs.append(candidateFileSource)
	callArgs.append("%s%s" % (candidateFileOutput, rtype))

	subprocess.call(callArgs)

	return 1

#######################################################################################


for arg in sys.argv:
	if arg == "-h" or arg == "--help":
		usage()
		exit(0)

if len(sys.argv) != 3:
	usage()
	exit(-1)

theTemplate    = sys.argv[1]
theID          = sys.argv[2]
json2robotPath = "%s/%s" % (os.path.dirname(sys.argv[0]), "json2robot")

if not os.path.isfile(json2robotPath):
	print "error: json2robot utility not found. looking for it here: " + json2robotPath
	exit(-1)

# print "processing %s --> %s.an per mood" % (theTemplate, theID)

# includes potential mis-spellings
knownMoods = [
		["happy"],
		["cautious"],
		["curious"],
		["frustrated"],
		["silly"],
		["brave"],
		["surprised", "suprised"]
	]


moodReplaceToken = "<MOOD>"
outFileSuffix = ".AN"

if not moodReplaceToken in theTemplate:
	print "base template does not contain mood token \"%s\": \"%s\": doing straight conversion." % (moodReplaceToken, theTemplate)
	outFN = "%s%s" % (theID, outFileSuffix)
	numConverted = tryConvert(theTemplate, outFN)
	if (numConverted == 1):
		exit(0)
	else:
		exit(1)

numConverted = 0
for moodSet in knownMoods:
	for mood in moodSet:
		candidateFileSource = theTemplate.replace(moodReplaceToken, mood)
		candidateFileOutput = "%s_%d%s" % (theID, knownMoods.index(moodSet), outFileSuffix)
		numConverted += tryConvert(candidateFileSource, candidateFileOutput)

if numConverted == 0:
	print "error: no files found to convert. %s" % (theTemplate)
	exit(-1)
else:
	exit(0)


