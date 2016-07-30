class SayController < ApplicationController
  def call
  end

  def get
  	@test = params[:foo]
  	require 'open3'
  	@status=Open3.capture2("C:\\ProgramData\\3CXPhone for Windows\\PhoneApp\\CallTriggerCmd.exe","-cmd","makecall:#{@test}");
  end
end
