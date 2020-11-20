import clr
clr.AddReferenceToFileAndPath(r"R2Core.dll")
clr.AddReferenceToFileAndPath(r"R2Core.Common.dll")
clr.AddReferenceToFileAndPath(r"R2Core.PushNotifications.dll")
clr.AddReferenceToFileAndPath(r"R2Core.Scripting.dll")
clr.AddReferenceToFileAndPath(r"R2Core.Video.dll")
clr.AddReferenceToFileAndPath(r"R2Core.Audio.dll")
from R2Core import *
import R2Core.PushNotifications
import R2Core.Scripting
import R2Core.Common
import R2Core.Video
import R2Core.Audio
clr.ImportExtensions(R2Core.Common)
clr.ImportExtensions(R2Core)
clr.ImportExtensions(R2Core.Network)
clr.ImportExtensions(R2Core.PushNotifications)
clr.ImportExtensions(R2Core.Scripting)
clr.ImportExtensions(R2Core.Video)
clr.ImportExtensions(R2Core.Audio)