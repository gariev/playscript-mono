#!/usr/bin/env python

import os
from os.path import *
from sys import argv, exit

if len(argv) <= 1:
	print "usage: %s filename [filename ...]" % basename(argv[0])
	exit(1)

# First sanity check the arguments
files = argv[1:]
for file in files:
	if isfile(file) != True:
		print "Error: '%s' is not a valid file" % file
		exit(1)

def processComments(line, commentBlock):
	line = line.strip()

	# Handle active block comments.
	if commentBlock:
		# If a comment block is active, and it doesn't contain a terminator, it
		# retmains active.
		if not "*/" in line:
			return (True, True)
		# We don't support a block comment terminator followed by non-
		# whitespace.
		if not line[-2:] == "*/":
			raise Exception("Unsupported syntax")
		# The comment block ends on this line.
		return (True, False)

	# If a comment block is not active, and the line begins with '//', then it
	# is a single line comment.
	if line[:2] == "//":
		return (True, False)

	# Handle starting new block comments.
	if "/*" in line:
		# Warning: this isn't correct, you could start a line with a comment
		# block and then close it, followed by actual code. It should suffice
		# for now, however.
		isCommentLine = line[:2] == "/*"
		isCommentOpen = False
		index = line.find("/*")
		while index >= 0:
			isCommentOpen = not isCommentOpen
			if isCommentOpen:
				index = line.find("*/", index + 2)
			else:
				index = line.find("/*", index + 2)
		return (isCommentLine, isCommentOpen)

	return (False, False)

for file in files:
	try:
		f = open(file, 'r+')
	except:
		print "Error: could not open '%s' for read/write" % file
		exit(1)

	# steps:
	# 1) find first non-comment/non-whitespace line
	# 2) add "package {"
	# 3) begin indenting all lines by a single tab
	# 4) skip over any import statement lines
	# 5) add "public class <filename>Test extends BaseTest {"
	# 6) begin indenting all lines 2 tabs
	# 7) add public static function Main():int {
	# 8) begin indenting all lines 3 tabs
	# 9) at the bottom of the file, add "\t\t\treturn results ();\n\t\t}\n\t}\n}""
	isCommentBlock = False
	inPackageBlock = False
	inClassBlock = False
	newLines = []
	skipReason = ""
	for line in f:
		if "package " in line:
			skipReason = "it already contains a package defintion"
			break

		if not inPackageBlock:
			try:
				isComment, isCommentBlock = processComments(line, isCommentBlock)
			except:
				skipReason = "it contains comment syntax that could not be parsed - " + line
				break

			if not isComment and not line.isspace():
				newLines.append("package {\n")
				inPackageBlock = True
			else:
				newLines.append(line)

		if inPackageBlock and not inClassBlock:
			if not line.isspace() and not line.strip()[:7] == "import ":
				base, ext = splitext(basename(file))
				newLines.append("\tpublic class " + base + "Test extends BaseTest {\n")
				newLines.append("\t\tpublic static function Main():int {\n")
				inClassBlock = True
			else:
				newLines.append("\t" + line)

		if inClassBlock:
			newLines.append("\t\t\t" + line)

	if skipReason != "":
		print "Skipping '%s' because %s." % (file, skipReason)
		f.close()
		continue

	newLines.append("\t\t\treturn results();\n")
	newLines.append("\t\t}\n")
	newLines.append("\t}\n")
	newLines.append("}\n")

	f.seek(0)
	f.write(''.join(newLines))
	f.close()
