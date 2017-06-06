#!/usr/bin/python

import json
import sys
import os



def usage():
	print "Prints all the string values in a json file"
	print "USAGE:"
	print "%s [--help] file.json" % (sys.argv[0])
	print "example: %s Dashlight.json" % (sys.argv[0])

def checkArgs():
	if len(sys.argv) != 2:
		usage()
		exit(-1)

	if sys.argv[1] == "--help":
		usage()
		exit(0)

def parseJson(filePath):
	fp = open(filePath, "r")
	if not fp:
		print "Error: could not open \"%s\"" % (filePath)
		exit(-1)

	fileContents = fp.read()
	fp.close()

	try:
		jsObj = json.loads(fileContents)
	except:
		print "Error: could not parse \"%s\"" % (filePath)
		exit(-1)

	return jsObj

def analyze(jsElement):

	if False:
		pass
	elif type(jsElement) is str:
		print jsElement.encode('utf-8')
	elif type(jsElement) is unicode:
		print jsElement.encode('utf-8')
	elif type(jsElement) is bool:
		pass
	elif type(jsElement) is int:
		pass
	elif type(jsElement) is float:
		pass
	elif type(jsElement) is dict:
		for key,value in jsElement.iteritems():
			analyze(value)
	elif type(jsElement) is list:
		for value in jsElement:
			analyze(value)
	else:
		print "Error: unhandled js type: %s" % (type(jsElement))


def main():
	checkArgs()
	jsContents = parseJson(sys.argv[1])
	analyze(jsContents)


main()

