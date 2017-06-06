#!/usr/bin/python

import sys
import urllib2
import json
import os.path

code_word = None

url_service_root = "https://alpha-share.makewonder.com"
url_service      = url_service_root + "/wonder/program"

def usage():
	print "downloads the shared wonder program and writes to the local directory"
	print "USAGE:"
	print "%s code words.." % (sys.argv[0])

def print_error(msg):
	print "error: %s" % (msg)

def parse_args():
	if len(sys.argv) < 2:
		usage()
		exit(-1)

	global code_word
	code_word = "-".join(sys.argv[1:])
	code_word = code_word.replace(" ", "-")

def download_step_1(code_word):
	url = url_service + "/" + code_word
	print "%s" % (url)

	try:
		response = urllib2.urlopen(url)
	except urllib2.HTTPError as e:
		print_error("%s on \"%s\"" % (e, url))
		exit(-1)

	if response.code != 200:
		print_error("HTTP %s on \"%s\"" % (response.code, url))
		exit(-1)


	try:
		jso = json.loads(response.read())
	except:
		print_error("could not parse json from %s" % (url))
		exit(-1)

	if 'url' not in jso:
		print_error("missing 'url' in response from %s" % (url))
		exit(-1)

	download_step_2(jso['url'])

def download_step_2(url):
	try:
		response = urllib2.urlopen(url)
	except urllib2.HTTPError as e:
		print_error("%s on \"%s\"" % (e, url))
		exit(-1)

	if response.code != 200:
		print_error("HTTP %s on \"%s\"" % (response.code, url))
		exit(-1)

	body = response.read()

	try:
		jso = json.loads(body)
	except:
		print_error("could not parse json from %s" % (url))
		exit(-1)

	if 'user_facing_name' not in jso:
		print_error("no 'user_facing_name' in response from %s" % (url))
		exit(-1)

	ufn = jso['user_facing_name']
	fn = ufn + ".txt"

	if os.path.isfile(fn):
		print_error("file already exists: \"%s\". please (re)move it and try again." % (fn))
		exit(-1)

	f = open(fn, "w")
	f.write(body)
	f.close()

	print "wrote to \"%s\"" % (fn)




def main():
	global code_word

	parse_args()

	download_step_1(code_word)


main()