# 3CXtest : Make call to call-center after test-web
Required: Installed 3CX phone system with fully account, which add voIP operator (SIP trunk) 
1. Install 3CX Softphone v14, download here: http://downloads.3cx.com/downloads/3CXPhoneforWindows14.msi
2. Run project CallTriggerCmdPlugin by Microsoft Visual Studio, this project will generate 3 binaries:

    CallTriggerCmdPlugin.dll – This is the Call trigger dll which is the plugin that uses the 3CXPhone API.
    CallTriggerCmd.exe – this is the executable to generate outbound call requests.
    CallTriggerCmdServiceProvider.dll – this is a common dependency DLL used to communicate the exe with the plug-in.

3. Follow this instruction: 

    a. Copy all 3 files into the 3CXPhone for Windows installation path – default is “C:\ProgramData\3CXPhone for Windows\PhoneApp”.
    b. Modify the 3CXPhone configuration file “3CXWin8Phone.user.config” and add the new plugin example:
    <add key=”CRMPlugin” value=”CallNotifier,3CXPhoneTapiPlugin,CallTriggerCmdPlugin“/>
    c. On startup, 3CXPhone for Windows will load the new plugin.
    d. Open a command prompt window and go to the 3CX Phone for Windows installation directory and type in the following command:
        CallTriggerCmd.exe -cmd makecall:DESTINATION_NUMBER

5. Run test_app(wrote by Ruby) on Rails. Get the link localhost/say/call on browser to enter beta-web. 
6. Take your phone number and click Call. The operator make a call to your phone.
(Require internet to call)
