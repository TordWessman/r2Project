import clr
import os
audio_lib = "R2Core.Audio.dll"
video_lib = "R2Core.Video.dll"

clr.AddReferenceToFileAndPath(r"R2Core.dll")
clr.AddReferenceToFileAndPath(r"R2Core.Common.dll")
clr.AddReferenceToFileAndPath(r"R2Core.Scripting.dll")

from R2Core import *
import R2Core.Scripting
import R2Core.Common
clr.ImportExtensions(R2Core.Common)
clr.ImportExtensions(R2Core)
clr.ImportExtensions(R2Core.Network)
clr.ImportExtensions(R2Core.Scripting)

if os.path.isfile(audio_lib):
	clr.AddReferenceToFileAndPath(audio_lib)
	clr.ImportExtensions(R2Core.Audio)

if os.path.isfile(video_lib):
	clr.AddReferenceToFileAndPath(video_lib)
	clr.ImportExtensions(R2Core.Video)