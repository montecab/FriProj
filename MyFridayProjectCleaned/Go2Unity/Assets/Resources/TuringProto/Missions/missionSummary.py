#!/usr/bin/python

import json
import sys
import os


class Constants:
	AUTOFIXED = "auto-fixed"

class Globals:
	currentChallengeName = ""
	currentChallengeStep = ""
	autoFixed = False


def usage():
	print "Prints summary of a mission."
	print "Also if a correctible error is found, makes a corrected copy in 'auto-fixed' folder."
	print "USAGE:"
	print "%s [--help] file.json" % (sys.argv[0])
	print "example: %s Dashlight.json" % (sys.argv[0])
	print "example: rm -f auto-fixed/* && find . -name \"*.json\" -print0 | xargs -0 -I{} %s {}" % (sys.argv[0])

def checkArgs():
	if len(sys.argv) != 2:
		usage()
		exit(-1)

	if sys.argv[1] == "--help":
		usage()
		exit(0)

def stdErr(msg):
	print("Error: %25s step %2d: %s" % (
		Globals.currentChallengeName,
		Globals.currentChallengeStep,
		msg))

def parseJson(filePath):
	fp = open(filePath, "r")
	if not fp:
		print "Error: could not open \"%s\"" % (filePath)
		exit(-1)

	fileContents = fp.read()
	fp.close()

	# todo: try/catch
	try:
		jsObj = json.loads(fileContents)
	except:
		print "Error: could not parse \"%s\"" % (filePath)
		exit(-1)

	return jsObj

def analyzeBehaviors(jsBehaviors):
	tallies = {}
	for jsBehavior in jsBehaviors:
		itemType = jsBehavior["type"]
		if not itemType in tallies:
			tallies[itemType] = 0
		tallies[itemType] += 1

	for key in tallies:
		print "Behavior: %30s: %d" % (key, tallies[key])
	print "NumBehaviors: %s" % (len(jsBehaviors))

def analyzeTransitions(jsTransitions):
	tallies = {}
	for jsTransition in jsTransitions:
		itemType = jsTransition["trigger"]["type"]
		if not itemType in tallies:
			tallies[itemType] = 0
		tallies[itemType] += 1

	for key in tallies:
		print "Cue: %30s: %d" % (key, tallies[key])
	print "NumTransitions: %s" % (len(jsTransitions))

def findState(jsProgram, stateID):
	for jsState in jsProgram["states"]:
		if jsState["id"] == stateID:
			return jsState

def findBehavior(jsProgram, id):
	for jsBehavior in jsProgram["behaviors"]:
		if jsBehavior["id"] == id:
			return jsBehavior

def triggerCanTransitionImmediately(trigType):
	triggers = [
		"TRAVELING_BACKWARD",
		"TRAVELING_FORWARD",
		"TRAVELING_STOPPED",
		"KIDNAP",
		"KIDNAP_NOT",
		"BEACON_SET",
		"DISTANCE_SET",
		"STALL",
		"STALL_NOT",
		"LEAN_LEFT",
		"LEAN_RIGHT",
		"LEAN_FORWARD",
		"LEAN_BACKWARD",
		"LEAN_UPSIDE_DOWN",
		"LEAN_UPSIDE_UP",
		"SLIDE_X_POS",
		"SLIDE_X_NEG",
		"SLIDE_Y_POS",
		"SLIDE_Y_NEG",
		"SLIDE_Z_POS",
		"SLIDE_Z_NEG",
		"DROP",
		"SHAKE",
		"IMMEDIATE",
	]
	return any(trigType in s for s in triggers)

def triggerIsMicrophone(trigType):
	triggers = [
		"CLAP",
		"VOICE",
	]
	return any(trigType in s for s in triggers)

# from ShowClapVoiceWarning() in trBehavior.cs
def behaviorMutesMicrophone(itemType):
	items = [
		"MOODY_ANIMATION",
		"SOUND_USER_1",
		"SOUND_USER_2",
		"SOUND_USER_3",
		"SOUND_USER_4",
		"SOUND_USER_5",
		"SOUND_USER",
		"SOUND_VOCAL_BRAVE",
		"SOUND_VOCAL_CAUTIOUS",
		"SOUND_VOCAL_CURIOUS",
		"SOUND_VOCAL_FRUSTRATED",
		"SOUND_VOCAL_HAPPY",
		"SOUND_VOCAL_SILLY",
		"SOUND_VOCAL_SURPRISED",
		"SOUND_ANIMAL",
		"SOUND_SFX",
		"SOUND_TRANSPORT",
		"MOVE_CONT_STRAIGHT",
		"MOVE_CONT_SPIN",
		"MOVE_DISC_STRAIGHT",
		"MOVE_DISC_TURN",
		"MOVE_TURN_VOICE",
	]
	return any(itemType in s for s in items)




def validateTransition(jsProgram, jsTransition):
	trigType = jsTransition["trigger"]["type"]
	stateSrc = findState(jsProgram, jsTransition["state_source_id"])

	if False: # this is no longer a problem.
		if triggerCanTransitionImmediately(trigType):
			# verify source state has activation count of 0
			if not stateSrc:
				return False
			ac = stateSrc["activation_time"]
			if ac > 0:
				stdErr("%30s has activation count %s and also an outgoing %s trigger." % (
					stateSrc["behavior_id"], ac, trigType))

				# do not auto-fix.
				# state["activation_time"] = 0
				# Globals.autoFixed = True

	if triggerIsMicrophone(trigType):
		behaviorSrc = findBehavior(jsProgram, stateSrc["behavior_id"])
		behaviorTypeSrc = behaviorSrc["type"]
		if (behaviorMutesMicrophone(behaviorTypeSrc)):
			stdErr("%35s has outgoing microphone trigger: %s" % (
				behaviorTypeSrc, trigType))

def validateState(jsProgram, state):
	itemType = state["behavior_id"]
	if itemType == "BEHAVIOR_OMNI":
		# verify activation count is zero
		ac = state["activation_time"]
		if ac > 0:
			stdErr("OMNI state has activation count %d. should be zero." % (ac))
			state["activation_time"] = 0
			Globals.autoFixed = True



def validate(jsProgram):
	for jsState in jsProgram["states"]:
		validateState(jsProgram, jsState)

	for jsTransition in jsProgram["transitions"]:
		validateTransition(jsProgram, jsTransition)

def validateHintRobotType(jsHint):
	if not "robot_type" in jsHint["program"]:
		if Globals.robotType != 1000:
			stdErr("no robot type in hint \"%s\"" % jsHint["user_facing_name"])
		hintRobotType = 1000
	else:
		hintRobotType = jsHint["program"]["robot_type"]

	if hintRobotType != Globals.robotType:
		stdErr("robot type mismatch. challenge is %d but hint \"%s\" is %d" % (Globals.robotType, jsHint["user_facing_name"], hintRobotType))
		if Globals.robotType != 1000:
			jsHint["program"]["robot_type"] = Globals.robotType
			Globals.autoFixed = True



def validateHint(jsHint):
	validateHintRobotType(jsHint)


def validateHints(jsPuzzle):
	for jsHint in jsPuzzle["hints"]:
		validateHint(jsHint)


def analyzePuzzle(jsPuzzle):
	print
	print
	print "Puzzle: \"%s\"" % (jsPuzzle["user_facing_name"])
	print "Description: \"%s\"" % (jsPuzzle["description"])
	print "IQ Points: %s" % (jsPuzzle["iq_points"])
	jsHints = jsPuzzle["hints"]
	if len(jsHints) == 0:
		stdError("no hints.")
		return


	validateHints(jsPuzzle)

	jsLastHint = jsHints[len(jsHints) - 1]
	jsProgram = jsLastHint["program"]
	analyzeBehaviors(jsProgram["behaviors"])
	analyzeTransitions(jsProgram["transitions"])
	validate(jsProgram)

def addWordsString(s):
	Globals.words.extend(s.split())

def addWordsHint(jsHint):
	if "description" in jsHint:
		addWordsString(jsHint["description"])

def addWordsPuzzle(jsPuzzle):
	addWordsString(jsPuzzle["user_facing_name"])
	addWordsString(jsPuzzle["description"])
	for jsHint in jsPuzzle["hints"]:
		addWordsHint(jsHint)

def addWordsChallenge(jsChallenge):
	addWordsString(jsChallenge["user_facing_name"])
	addWordsString(jsChallenge["description"])
	for jsPuzzle in jsChallenge["puzzles"]:
		addWordsPuzzle(jsPuzzle)


def analyze(jsContent):
	if not "user_facing_name" in jsContent:
		print "Error: %s has no user-facing name." % (sys.argv[1])
		exit(-1)

	Globals.currentChallengeName = jsContent["user_facing_name"]
	Globals.currentChallengeStep = 0
	Globals.autoFixed            = False
	Globals.robotType            = 1000		# unknown
	Globals.words                = []

	if "type" in jsContent:
		rt = jsContent["type"]
		if rt == 0:
			Globals.robotType = 1001
		elif rt == 1:
			Globals.robotType = 1002
		else:
			stdErr("unhandled robot type: %d" % (rt))
	else:
		stdErr("no robot type.")

	jsPuzzles = jsContent['puzzles']
	if not jsPuzzles:
		stdErr("no puzzles")
		exit(-1)

	print "Analysis:"

	iqPoints = 0;
	for jsPuzzle in jsPuzzles:
		Globals.currentChallengeStep += 1
		analyzePuzzle(jsPuzzle)
		iqPoints += jsPuzzle["iq_points"]

	addWordsChallenge(jsContent)

	print
	print "Summary for \"%s\"" % (jsContent["user_facing_name"])
	print "Description: \"%s\"" % (jsContent["description"].encode('utf-8'))
	print "Robot Type: %d" % (Globals.robotType)
	print "NumPuzzles: %s" % (len(jsPuzzles))
	print "Total IQ Points: %s" % (iqPoints)
	print "Total word count: %d" % (len(Globals.words))
	print "Words: %s" % (" ".join(Globals.words).encode('utf-8'))

	if Globals.autoFixed:
		if not os.path.exists(Constants.AUTOFIXED):
			os.makedirs(Constants.AUTOFIXED)

		fixedFilePath = "%s/%s" % (Constants.AUTOFIXED, os.path.basename(sys.argv[1]))
		print "saving fixed file to \"%s\"." % (fixedFilePath)
		fp = open(fixedFilePath, "w")
		fp.write(json.dumps(jsContent, indent=2))
		fp.close()



def main():
	checkArgs()
	jsContents = parseJson(sys.argv[1])
	analyze(jsContents)


main()

