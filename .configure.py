#!/usr/bin/env python

import os, re, sys
import subprocess

if sys.platform in ('cygwin', 'win32'):
	newline = "\r\n"
else:
	newline = "\n"

rootPath = None
	# This will be filled in.

#strataTempDirName = "Temp_Strata"
	# We create this in the project directory next to "Temp" and "Assets"
strataTempDirName = None
	# I thought I needed this - now I don't.

supportedPlatforms = ("daydream", "hololens", "steamvr")
	# Add any newly supported platforms to this list.
	# Folders with these names will show up in /shared_assets and /configuration
#supportedPlatforms = []
	# I thought I needed this - now I don't.

ignorePlatformConfigFiles = ["readme"]
	# A list of files to ignore in the configuration directories,
	# using lowercase filenames without any extension.

def logString(str, err = False):
	#if err:
	#	sys.stderr.write(str)
	#	return
	sys.stdout.write(str)

def pressAnyKey(err = False):
	if err:
		logString("\n**************************\n*** ERROR ENCOUNTERED! ***\n**************************\n(press return...)\n", True)
	else:
		logString("\n(press return...)\n", False)
	sys.stdin.readline()

def createLink(dstDir, dstName, srcPath):
	dst = dstName.strip()
	if len(dst) == 0:
		return None
	#dstPath = os.path.join(dstDir, dst)
	#dstDir = os.path.dirname(dstPath)
	#dst = os.path.basename(dstPath)
		# We need to keep within one directory when processing a symlinks.txt file
		# or we can't easily cleanup after ourself. Put another symlinks.txt file
		# in a nested directory if you want links in there.
	wasDir = os.getcwd()
	try:
		if not os.path.isdir(dstDir):
			os.makedirs(dstDir)
		os.chdir(dstDir)
		src = srcPath.strip()
		#logString("  %s: %s\n" % (dst, src))
		if sys.platform == "darwin":
			os.symlink(src, dst)
		else:
			srcWin = src.replace("/", "\\")
			if os.path.isdir(srcWin):
				subprocess.call(["cmd.exe", "/C", "mklink", "/D", dst, srcWin])
			else:
				subprocess.call(["cmd.exe", "/C", "mklink", dst, srcWin])
		if not os.path.exists(os.path.join(dstDir, dst)):
			logString("bad symbolic link: %s <==> %s\n" % (src, dst))
			raise Exception("bad symbolic link specification")
	except:
		logString("caught exception: %s\n" % sys.exc_info()[0], True)
		os.chdir(wasDir)
		raise
	return dst

def appendLinked(linked, makeDefs):
	with open(makeDefs, "ab") as f:
		f.writelines("#define STRATA_LINKED_%s // true%s" % (re.sub(r"[^a-zA-Z0-9_]", "_", linked, newline)))

def makePlatformDependent(linked, platform):
	if len(supportedPlatforms) == 0:
		return []
	dir = os.path.dirname(linked)
	filename = os.path.basename(linked)
	tagFile = os.path.join(dir, "." + filename + ".platform_" + platform)
	with open(tagFile, "wb") as f:
		f.writelines("// The %s symbolic link is only meant for the %s platform%s" % (os.path.basename(linked), platform, newline))
	return [os.path.basename(tagFile)]

def createPlatformSpecificFiles(baseDir):
	global rootPath
	global strataTempDirName
	global supportedPlatforms
	global ignorePlatformConfigFiles
	if strataTempDirName is not None:
		tempDir = os.path.join(os.path.dirname(baseDir), strataTempDirName)
		if not os.path.isdir(tempDir):
			os.makedirs(tempDir)
	if len(supportedPlatforms) == 0:
		return []
	relativeToPath = os.path.abspath(baseDir)
	addToIgnoreFiles = []
	for platform in supportedPlatforms:
		configSrc = os.path.join(rootPath, "configuration", platform);
		if os.path.isdir(configSrc):
			configDst = os.path.join(baseDir, "config_" + platform);
			for scan in os.listdir(configSrc):
				if os.path.splitext(scan)[0].lower() in ignorePlatformConfigFiles:
					continue
				fullSrcPath = os.path.join(configSrc, scan)
				relativeSrcPath = os.path.relpath(fullSrcPath, relativeToPath)
				linked = createLink(baseDir, scan + ".config_" + platform, relativeSrcPath)
				if linked is not None:
					#addToIgnoreFiles.append(linked)
						# because this is a symbolic link it'll already be ignored.
					addToIgnoreFiles.extend(makePlatformDependent(os.path.join(baseDir, scan), platform))
	return addToIgnoreFiles

def parseSymLinks(baseDir, srcName, ignoreFiles, makeDefs, metas, forPlatform):
	global supportedPlatforms
	with open(os.path.join(baseDir, srcName)) as f:
		#logString("creating symbolic links:\n")
		for line in f:
			#logString("line: %s\n" % line)
			if line.strip().startswith("#"):
				includeRelPath, found = re.subn(r"\s*#\s*include\s*<\s*([^>]+)\s*>\s*", r"\1", line)
				if found > 0:
					if os.path.isfile(os.path.join(baseDir, includeRelPath)):
						parseSymLinks(baseDir, includeRelPath, ignoreFiles, makeDefs, metas, forPlatform)
					else:
						raise Exception("include file not found: %s" % includeRelPath)
				continue
			parts = line.split(":")
			if len(parts) < 2:
				raise Exception("bad line format")
			dstName = parts[0].strip()
			srcPath = parts[1].strip()
			if len(parts) > 2:
				platform = parts[2].strip().lower()
				if not platform in supportedPlatforms:
					raise Exception("parsed an unsupported platform")
				if (forPlatform is not None) and (platform != forPlatform):
					continue
				# We no longer try to mark up the directories with tag files.
				# Between Unity and VIsual Studio it just won't work.'
				platform = None
			else:
				platform = None
			if dstName == "*":
				if os.path.basename(srcPath) != "*":
					raise Exception("bad wildcard usage")
				srcPath = os.path.dirname(srcPath)
				dstNames = []
				srcPaths = []
				for scan in os.listdir(os.path.join(baseDir, srcPath)):
					dstNames.append(scan)
					srcPaths.append(os.path.join(srcPath, scan))
			else:
				dstNames = (dstName,)
				srcPaths = (srcPath,)
			for dstName, srcPath in zip(dstNames, srcPaths):
				linked = createLink(baseDir, dstName, srcPath)
				if linked is not None:
					if makeDefs is not None:
						appendLinked(linked, makeDefs)
					if platform is not None:
						ignoreFiles.extend(makePlatformDependent(linked, platform))
					linked += ".meta"
					if linked in metas:
						del metas[linked]

def updateSymLinks(baseDir, srcName, foundAssets, forPlatform):
	global supportedPlatforms
	metas = {}
	for scan in os.listdir(baseDir):
		existing = os.path.join(baseDir, scan)
		if os.path.islink(existing):
			if os.path.exists(existing + ".meta"):
				metas[scan + ".meta"] = True
			os.remove(existing)
	ignoreFiles = []
	makeDefs = None
	if (not foundAssets) and (os.path.basename(baseDir) == "Assets"):
		foundAssets = True
		if forPlatform is not None:
			raise Exception("expected to define the platform once we find the assets")
		projectName = os.path.basename(os.path.dirname(baseDir))
		matchPlatform = projectName.lower()
		for findPlatform in supportedPlatforms:
			if matchPlatform.endswith("_" + findPlatform):
				logString("#\n# Preparing Unity project %s for %s platform\n#\n" % (projectName, findPlatform))
				forPlatform = findPlatform
				break
		if forPlatform is None:
			logString("#\n# Preparing Unity project %s as platform independent\n#\n" % projectName)
		#ignoreFiles = createPlatformSpecificFiles(baseDir)
			# no longer any need or support for this mechanism
		makeDefs = os.path.join(baseDir, "symlinks.cs")
		if os.path.isfile(makeDefs):
			os.remove(makeDefs)
		makeDefs = None # I changed my mind about this file.
	parseSymLinks(baseDir, srcName, ignoreFiles, makeDefs, metas, forPlatform)
	if len(metas) > 0:
		logString("removing extra files:\n")
		for key in metas.keys():
			removing = os.path.join(baseDir, key)
			logString("  %s\n" % removing)
			os.remove(removing)
	return foundAssets, forPlatform, ignoreFiles

def updateGitIgnore(findIn, ignoreFiles):
	#logString("here -> (%s, %s)\n" % (findIn, ignoreFiles))
	gitIgnore = os.path.join(findIn, ".gitignore")
	writeLines = []
	if os.path.exists(gitIgnore):
		logString("fixing up existing .gitignore:\n")
		with open(gitIgnore, "r") as f:
			for line in f:
				if line.lstrip().startswith("#") and line.find("ignore symbolic links"):
					break
				writeLines.append(line)
	elif len(ignoreFiles) == 0:
		return
	logString("rewriting existing .gitignore entries: %s\n" % str(writeLines))
	logString("writing new .gitignore entries: %s\n" % str(ignoreFiles))
	with open(gitIgnore, "w+b") as f:
		f.writelines(writeLines)
		if len(ignoreFiles) > 0:
			f.write("# ignore symbolic links and configuration files (these must come last in this file)%s" % newline)
			for link in ignoreFiles:
				f.write("/%s%s" % (link, newline))
		f.truncate()

def findSymLinks(findIn, exclude = None, foundAssets = False, forPlatform = None):
	#logString("finding(%s)\n" % findIn)
	if exclude is None:
		exclude = []
	srcName = "symlinks.txt"
	doUpdate = os.path.isfile(os.path.join(findIn, srcName))
	if not doUpdate:
		srcName = "." + srcName
		doUpdate = os.path.isfile(os.path.join(findIn, srcName))
	ignoreFiles = []
	foundProjects = []
	if doUpdate:
		wasFoundAssets = foundAssets
		foundAssets, forPlatform, ignoreFiles = updateSymLinks(findIn, srcName, foundAssets, forPlatform)
		if foundAssets != wasFoundAssets:
			# Record that we found a Unity project
			foundProjects.append(findIn)
	foundLinks = []
	for scan in os.listdir(findIn):
		matching = scan.lower()
		if matching in exclude:
			continue
		descend = os.path.join(findIn, scan)
		#logString("checking(%s, %s)\n" % (descend, scan))
		if os.path.islink(descend):
			foundLinks.append(scan)
		elif os.path.isdir(descend):
			foundProjects.extend(findSymLinks(descend, None, foundAssets, forPlatform))
	if doUpdate:
		try:
			ignoreFiles.extend(foundLinks)
			#logString("updateGitIgnore(%s, %s)\n" % (findIn, ignoreFiles))
			updateGitIgnore(findIn, ignoreFiles)
		except:
			logString("caught exception: %s\n" % str(sys.exc_info()), True)
			raise
	return foundProjects

def validateSymLinks(findIn, found):
	for scan in os.listdir(findIn):
		fullpath = os.path.join(findIn, scan)
		if os.path.islink(fullpath):
			savepath = os.path.realpath(fullpath)
			if found.has_key(savepath):
				logString("\nfound duplicate symlinks:\n  %s\n and:\n %s\n pointing to:\n%s\n" % (found[savepath], fullpath, savepath))
				raise Exception("duplicate symlink directory found!")
			found[savepath] = fullpath
		elif os.path.isdir(fullpath):
			validateSymLinks(fullpath, found)

try:
	if False and sys.platform == "cygwin" and (len(sys.argv) < 2 or sys.argv[1] != "sudo"):
		out = subprocess.check_output(["cygstart", "--action=runas", "--wait", "--shownormal", "python.exe", "-u", sys.argv[0], "sudo"])
		err = ""
		#p = subprocess.Popen(["cygstart", "--action=runas", "--wait", "--verbose", "python.exe", "-u", sys.argv[0], "sudo"], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
		#out, err = p.communicate()
		logString(out)
		logString(err, True)
			# This won't do anything. I just can't get this command to return any piped output!
	else:
		logString("#\n# Creating symbolic link files with administrator privileges...\n")
		if sys.platform == "win32" and (len(sys.argv) < 2 or sys.argv[1] != "sudo"):
			logString("# (be sure you're running with administrator privileges)\n")
		rootPath = os.path.abspath(os.path.dirname(sys.argv[0]))
		os.chdir(rootPath)
		try:
			foundProjects = findSymLinks(rootPath, (".git", "submodules"))
				# The exclusions list should use lower case for each item.
			#validateSymLinks(rootPath)
				# With multiple Unity projects now possible we need to limit our scanning
			logString("#\n# Validating Unity projects:\n#\n")
			for project in foundProjects:
				logString("  => %s\n" % project)
				validateSymLinks(project, {})
		except:
			logString("caught exception: %s\n" % str(sys.exc_info()), True)
			raise
		logString("#\n# Finished\n#\n")
		pressAnyKey()
except:
	logString("caught exception: %s\n" % str(sys.exc_info()), True)
	pressAnyKey(True)
	raise
