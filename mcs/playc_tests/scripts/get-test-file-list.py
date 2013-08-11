#!/usr/bin/env python
#
# get-test-file-list.py
#
# This script parses a list of directories and returns the file names of all
# the tests contained in them. Its contents are mostly from the tamarin-redux
# project (since that is where the tests came from).
#

import os
from os.path import *
from sys import argv, exit

class TestUtil(object):
	sourceExt = ".as"
	supportFolderExt = '_support'
	exclude = []

	def getTestsList(self, folders):
		'''Get all possible tests to run, then parse it down depending on
			configuration.  Returns list of tests to run'''
		# Gather the list of all possible tests
		fileExtentions = self.sourceExt
		for i in range(len(folders)):
			if folders[i][-1] == '/':
				folders[i] = folders[i][:-1]
			if folders[i].startswith('./'):
				folders[i] = folders[i][2:]
		tests = [a for a in folders if isfile(a) and self.istest(a, fileExtentions)]
		for a in [d for d in folders if isdir(d) and not (basename(d) in self.exclude)]:
			for d, dirs, files in os.walk(a, followlinks=True):
				if d.startswith('./'):
					d=d[2:]
				tests += [(d+'/'+f) for f in files if self.istest(f, fileExtentions)]
				# utilDirs contains all dirs that hold support files, and therefore
				# are excluded from the tests list
				# There are three kinds of util directories:
				# 1. directory with the same name as the test: all files in that dir
				#    are included when compiling the test
				# 2. directory with the same name as the test + _support (string is defined in self.supportFolderExt):
				#    all files in that dir are compiled, but not run - these files are
				#    normally passed in as args to the test itself
				# 3. directory with the name "includes": all files in that dir
				#    are manually included in test media and will not be compiled or run
				utilDirs = [ud for ud in dirs if (ud+self.sourceExt in files) or
						 (ud.endswith(self.supportFolderExt) and
						  ud[:-len(self.supportFolderExt)]+self.sourceExt in files)
						 or ud=="includes"]
				for x in [x for x in self.exclude+utilDirs if x in dirs]:
					dirs.remove(x)
		tests=[a for a in tests if os.path.dirname(a)!='' and os.path.dirname(a)!='.']
		return tests

	def istest(self, f, fileExtentions):
		return f.endswith(fileExtentions) and not f.endswith('Util'+self.sourceExt)

if __name__ == "__main__":
	if len(argv) != 2:
		print "usage: %s test_folder" % basename(argv[0])
	folder = argv[1]
	if isdir(folder) != True:
		print "error: %s is not a folder" % folder
	util = TestUtil()
	tests = util.getTestsList([folder])
	for test in tests:
		print test
