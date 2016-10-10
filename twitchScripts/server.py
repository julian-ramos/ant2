from OSC import OSCServer
import sys
from time import sleep

server = OSCServer( ("127.0.0.1", 7110) )
server.timeout = 0
run = True

# this method of reporting timeouts only works by convention
# that before calling handle_request() field .timed_out is 
# set to False
def handle_timeout(self):
    self.timed_out = True

# funny python's way to add a method to an instance of a class
import types
server.handle_timeout = types.MethodType(handle_timeout, server)

def monster_callback(path, tags, args, source):
   
    print ("Now do something with", path,float(args[0]),float(args[1])) 

def quit_callback(path, tags, args, source):
    # don't do this at home (or it'll quit blender)
    global run
    run = False

server.addMsgHandler( "/arrow/", monster_callback )
server.addMsgHandler( '/a/', monster_callback )
server.addMsgHandler( "/b/", monster_callback )
server.addMsgHandler( "/c/", monster_callback )
server.addMsgHandler( "/d/", monster_callback )

# user script that's called by the game engine every frame
def each_frame():
    # clear timed_out flag
    server.timed_out = False
    # handle all pending requests then return
    while not server.timed_out:
        server.handle_request()

# simulate a "game engine"
while run:
    # do the game stuff:
    sleep(1)
    # call user script
    each_frame()

server.close()