import OSC
c = OSC.OSCClient()
c.connect(('127.0.0.1', 7110))   
oscmsg = OSC.OSCMessage()
oscmsg.setAddress("/a")
oscmsg.append([10.0,20.0,3])
c.send(oscmsg)