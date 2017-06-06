#!/usr/bin/python

import datetime
import json
import subprocess
import sys

from collections import OrderedDict


class Globals:
	filename_out = "../Strings/wonder_subtitles_en.po.txt"

class Constants:
	filename_in = "Subtitles.json"

class Tokens:
	CAPTION       = "caption"
	CAPTIONS      = "captions"
	CAPTION_END   = "caption_end"
	CAPTION_START = "caption_start"
	FILE_NAME     = "file_name"
	SUBTITLES     = "subtitles"

def main():
	check_args()
	do_it()

###########################################
# Logging

def log(level_str, msg):
	sys.stderr.write("%s: %s\n" % (level_str, msg))

def log_e(msg):
	log("error", msg)

def log_w(msg):
	log("warning", msg)


###########################################
# Command-Line Parsing

def usage():
	print "Extracts user-facing strings from wonder video subtitle file."

# if 'arg' is provided, prints an error as if an argument were missing for arg.
# ie, it behaves as if you did "tail -n" instead of "tail -n 1"
def consume_one_arg(args, arg):
	if len(args) == 0:
		usage()
		if arg:
			error("\nMissing parameter for %s" % arg)
		exit(1)
	else:
		return args.pop(0)

def check_args():
	local_args = list(sys.argv)
	local_args.pop(0)

	while len(local_args) > 0:
		arg = consume_one_arg(local_args, None)

		if False:
			pass

		elif arg == "--help":
			usage()
			exit(0)

		else:
			usage()
			log_e("recognized argument: %s" % (arg))
			exit(0)

def parse_json(file_path):
	fp = open(file_path, "r")
	if not fp:
		print "Error: could not open for read: \"%s\"" % (file_path)
		exit(1)

	file_contents = fp.read()
	fp.close()

	try:
		js_obj = json.loads(file_contents, object_pairs_hook=OrderedDict)
	except:
		e = sys.exc_info()[0]
		print "Error: could not parse \"%s\" - %s" % (file_path, e)
		exit(1)

	return js_obj


def git_branch():
	return subprocess.check_output(['git', 'rev-parse', '--abbrev-ref', 'HEAD']).strip()
def git_hash():
	return subprocess.check_output(['git', 'rev-parse', 'HEAD']).strip()
def git_has_local_changes():
	output = subprocess.check_output(['git', 'diff-index', 'HEAD', '--', '.']).strip()
	return output != ""

def output_translation_header(fp):
	local_changes = ''
	if git_has_local_changes():
		local_changes = ' with local changes'
	else:
		local_changes = ' without local changes'

	fp.write('#. note this content is static and therefore has only one form, but we use the standard english Plural-Forms line anyhow to simplify runtime ingestion.\n')
	fp.write('#. from mission files.\n')
	fp.write('#. git branch:%s hash:%s%s\n' % (git_branch(), git_hash(), local_changes))
	fp.write('#. time ')
	fp.write('{:%Y-%m-%d %H:%M:%S}\n'.format(datetime.datetime.now()))
	fp.write('# smartling.placeholder_format_custom = \{[0-9]+\}\n')
	fp.write('msgid ""\n')
	fp.write('msgstr ""\n')
	fp.write('"Content-Type: text/plain; charset=UTF-8\\n"\n')
	fp.write('"Content-Transfer-Encoding: 8bit\\n"\n')
	fp.write('"Plural-Forms: nplurals=2; plural=n != 1;\\n"')
	fp.write('\n')

def encode_po_string(str):
	ret = str
	# ret = ret.replace("\\", "\\\\")
	ret = ret.replace("\n", " ")
	ret = ret.strip()
	ret = ret.replace("\"", "\\\"")
	return ret.encode("utf-8")

def output_po_string(fp_out, str, filename, str_type):
	str_encoded = encode_po_string(str)
	fp_out.write("\n")
	fp_out.write("#: source file: %s - string type: %s\n" % (filename, str_type))
	fp_out.write("#. source file: %s - string type: %s\n" % (filename, str_type))
	fp_out.write("msgid \"%s\"\n"  % str_encoded)
	fp_out.write("msgstr \"%s\"\n" % str_encoded)

############################################################

def do_it():
	fp_out = open(Globals.filename_out, "w")
	if not fp_out:
		log_e("Error: could not open for write: \"%s\"" % (Globals.filename_out))
		exit(1)

	output_translation_header(fp_out)

	js_master = parse_json(Constants.filename_in)
	js_subs = js_master[Tokens.SUBTITLES]
	for js_sub in js_subs:
		for cap in js_sub[Tokens.CAPTIONS]:
			desc = "subtitle. start: %ds finish:%ds" % (cap[Tokens.CAPTION_START], cap[Tokens.CAPTION_END])
			output_po_string(fp_out, cap[Tokens.CAPTION], js_sub[Tokens.FILE_NAME], desc)


main()
