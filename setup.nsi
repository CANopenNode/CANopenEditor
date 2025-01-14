; example1.nsi
;
; This script is perhaps one of the simplest NSIs you can make. All of the
; optional settings are left to their default settings. The installer simply 
; prompts the user asking them where to install, and drops a copy of example1.nsi
; there. 

;--------------------------------


!include "MUI2.nsh"
!include "x64.nsh"                  ; Macros for x64 machines

; The name of the installer
Name "OpenEdsEditor"

; The file to write
OutFile "edseditor-Setup.exe"

; Show install details
ShowInstDetails show

; The default installation directory
InstallDir "$PROGRAMFILES\OpenEdsEditor\"

; Request application privileges for Windows Vista
;RequestExecutionLevel Admin

SetOverwrite on


!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Header\nsis.bmp" ; optional
!define MUI_ABORTWARNING
  
;--------------------------------

; Pages


!insertmacro MUI_PAGE_LICENSE "License-GPLv3.txt"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES



;Page instfiles

; The stuff to install
Section "OpenEdsEditor" Secopeneds ;No components page, name is not important

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  File EDSEditorGUI\bin\Release\EDSEditor.exe
  File EDSEditorGUI\bin\Release\EDSEditor.exe.config
  File EDSEditorGUI\bin\Release\*.dll
  File EDSEditorGUI\bin\Release\style.css
  File Index_8287_16x.ico
  File License-GPLv3.txt
  
  SetOutPath $INSTDIR\Profiles
  File EDSEditorGUI\Profiles\*
   
  SetShellVarContext all
  CreateDirectory "$SMPROGRAMS\OpenEDSEditor"
  CreateShortCut "$SMPROGRAMS\OpenEDSEditor\OpenEDSEditor.lnk" $INSTDIR\EDSEditor.exe "" $INSTDIR\Index_8287_16x.ico 0
     
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
  CreateShortCut "$SMPROGRAMS\OpenEDSEditor\Uninstall.lnk" $INSTDIR\Uninstall.exe
  
SectionEnd ; end the section

;Language strings
LangString DESC_Secopeneds ${LANG_ENGLISH} "The Open EDS editor"

 
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
!insertmacro MUI_DESCRIPTION_TEXT ${Secopeneds} $(DESC_Secopeneds)
!insertmacro MUI_FUNCTION_DESCRIPTION_END


Function .onInit

  ;Extract InstallOptions files
  ;$PLUGINSDIR will automatically be removed when the installer closes
  
  InitPluginsDir
  
  Push $0
  Pop $0
  
FunctionEnd


Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...
  
  Delete "$INSTDIR\*"
  Delete "$INSTDIR\Profiles\*"
  RMDir "$INSTDIR\Profiles"
  RMDir "$INSTDIR"
  
  SetShellVarContext all

  Delete "$SMPROGRAMS\OpenEDSEditor\OpenEDSEditor.lnk" 
  RMDir "$SMPROGRAMS\OpenEDSEditor"

SectionEnd


