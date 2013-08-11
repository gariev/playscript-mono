# Makefile
#
# A build system for executing PlayScript tests on GNU Make (v3.81+)
#

# sh is the default $(SHELL) for GNU Make.
#
# bash is a credible alternative which I often leverage in Makefiles
#
#   echo -e
#   [[
#
SHELL := bash

# If you're not doing an out-of-the-box c/flex/bison textbook project,
# I find that the automatic suffix targets produce confusion.
#
# For larger-scale and sometimes-confusing projects, I prefer to code
# it all by hand than use the invisible magic.
#
.SUFFIXES:

DESTDIR := .build

# Add newline define for pretty printing where needed
define \n


endef

.PHONY: all  # the default: does any necessary building, but doesn't 'test'
all: $(DESTDIR)/all.ok
$(DESTDIR)/all.ok:
	@mkdir -p $(dir $@)
	touch $@

.PHONY: clean
clean:
	rm -rf $(DESTDIR)

PS_HOME			:= /Users/cbroglie/playscript-install
PS_BIN			:= $(PS_HOME)/bin
PS_LIB			:= $(PS_HOME)/lib/mono/4.0

MCS				:= $(PS_BIN)/mcs

MCS_LIB_NAMES	:= mscorlib.dll
MCS_LIB_NAMES	+= System.dll
MCS_LIB_NAMES	+= System.Core.dll
MCS_LIB_NAMES	+= System.Drawing.dll
MCS_LIB_NAMES	+= System.Xml.dll
MCS_LIB_NAMES	+= ICSharpCode.SharpZipLib.dll
MCS_LIB_NAMES	+= System.Json.dll
MCS_LIB_NAMES	+= System.Web.dll
MCS_LIB_NAMES	+= Playscript.Dynamic_aot.dll
MCS_LIB_NAMES 	+= pscorlib_aot.dll

MCS_LIBS		:= $(MCS_LIB_NAMES:%=$(PS_LIB)/%)

MCS_OPTS		:= $(MCS_LIBS:%=-r:%)
MCS_OPTS		+= -noconfig
MCS_OPTS		+= -nostdlib
MCS_OPTS		+= -sdk:4
MCS_OPTS		+= -nologo
MCS_OPTS		+= -debug:full
MCS_OPTS		+= -optimize-
MCS_OPTS		+= -codepage:utf8
MCS_OPTS		+= -unsafe
MCS_OPTS		+= -define:DEBUG
MCS_OPTS		+= -t:exe

ALL_TEST_FILES	:= $(shell ./scripts/get-test-file-list.py as3)
BAD_TEST_FILES	:= as3/AbcDecoder/accessSpecifiers.as
TEST_FILES		:= $(filter-out $(BAD_TEST_FILES),$(ALL_TEST_FILES))

TEST_FILE_NAMES	:= $(TEST_FILES:%.as=%)

.PHONY: test  # run the tests
test: $(DESTDIR)/test.ok
$(DESTDIR)/test.ok: $(TEST_FILE_NAMES:%=$(DESTDIR)/%.exe)
$(DESTDIR)/test.ok: $(TEST_FILE_NAMES:%=run-%)
	@mkdir -p $(dir $@)
	@echo  Built all tests
	touch $@

# define_compile_test
#
# Define a target to compile an individual .as test file
#
# $(1) name of the test
#
define define_compile_test

ADDITIONAL_FILES_$(1) := BaseTest.as
ADDITIONAL_FILES_$(1) += $(shell find $(dir $(1))$(notdir $(1)) -name '*.as' 2> /dev/null)
ADDITIONAL_FILES_$(1) += $(shell find $(dir $(1))$(notdir $(1))_support -name '*.as' 2> /dev/null)

$(DESTDIR)/$(1).exe: $(1).as $$(ADDITIONAL_FILES_$(1)) $(MCS_LIBS) $(MCS)
$(DESTDIR)/$(1).exe: $(MCS_LIB_NAMES:%=$(DESTDIR)/$(dir $(1))%)
$(DESTDIR)/$(1).exe:
	@mkdir -p $$(dir $$@)
	rm -f $$@
	$(MCS) \
$(MCS_OPTS:%=\${\n}		%) \${\n}		-out:$$@ \
		$(1).as $$(ADDITIONAL_FILES_$(1))
	test -e $$@

endef

$(foreach test,$(TEST_FILE_NAMES),$(eval $(call define_compile_test,$(test))))


# define_copy_libs
#
# Define a target to copy the libs to the build folder for each test
#
# $(1) folder to copy the libs to
#
define define_copy_libs

$(MCS_LIB_NAMES:%=$(DESTDIR)/$(1)%): $(DESTDIR)/$(1)%: $(PS_LIB)/%
	@mkdir -p $$(dir $$@)
	cp $$^ $$@

endef

$(foreach folder,$(sort $(dir $(TEST_FILE_NAMES))),$(eval $(call define_copy_libs,$(folder))))

# define_run_test
#
# Define a target to execute an individual test
#
# $(1) name of the test
#
define define_run_test

.PHONY .NOTPARALLEL: run-$(1)
run-$(1): $(DESTDIR)/$(1).exe
run-$(1):
	mono $(DESTDIR)/$(1).exe
endef

$(foreach test,$(TEST_FILE_NAMES),$(eval $(call define_run_test,$(test))))