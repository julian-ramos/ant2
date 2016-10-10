import string
import random
from pyfiglet import Figlet
import string
import csv
import time
import operator
from Readv3 import getUser, getMessage, getChannelname, getBannedUser, getBannedChannelname
from Readv3 import getslowmode, getr9k, getsubmode, getroomstatechannelname
from Readv3 import getOwner, getTurbo, getSub, getMod
from Socketv2 import openSocket, sendMessage,channelNum
from Settingsv2 import HOST, PORT, PASS, IDENT
from datetime import datetime
import OSC
import numpy
c = OSC.OSCClient()
c.connect(('127.0.0.1', 7110))   



monsters={'a':{'xcord':0,"ycord":0},'b':{'xcord':0,"ycord":0},'c':{'xcord':0,"ycord":0},'d':{'xcord':0,"ycord":0}}
arrow={"xcord":0,"ycord":0}



def emo():
	emos=["DOOMGuy","EagleEye", "EleGiggle","DansGame","4Head"]
	i=random.randint(0,2)
	return emos[i]

# Actually joins the rooms
s = openSocket()
f = Figlet(font='graceful')
pollStarted=False
### joinRoom(s)
readbuffer = ""
cue=''


id = 0


poll={}
wdf=False


# Sets how long the scraper will run for (in seconds)
starttime = time.time() + 7200



# Runs until time is up
while time.time() < starttime:
	
		# Pulls a chunk off the buffer, puts it in "temp"
		readbuffer = readbuffer + s.recv(1024)
		temp = string.split(readbuffer, "\n")
		readbuffer = temp.pop()
	
		# Iterates through the chunk
		for line in temp:
			print line
			id = id + 1
		
			# Parses lines and writes them to the file
			if "PRIVMSG" in line:
				try:

					# Gets user, message, and channel from a line
					user = getUser(line)
					message = getMessage(line)
					channelname = getChannelname(line)
					owner = getOwner(line)
					mod = getMod(line)
					sub = getSub(line)
					turbo = getTurbo(line)
					chan=channelNum(line)
					message=message.lower()
					if owner == 1:
						mod = 1
		
					# Writes Message ID, channel, user, date/time, and cleaned message to file
					with open('outputlog.csv', 'ab') as fp:
						ab = csv.writer(fp, delimiter=',')
						data = [id, channelname, user, datetime.now(), message.strip(), owner, mod, sub, turbo];
						ab.writerow(data)
						
					if "!monster" in  message:
						opts=message.split("monster")[1]
						opts=opts.strip().split(" ")
						if len(opts)==3 and opts[0] in monsters:
							if abs(float(opts[1]))<=30:
								
								if abs(monsters[opts[0]]['xcord']+float(opts[1]))>=150:
									monsters[opts[0]]['xcord']=150
								else:
									monsters[opts[0]]['xcord']+=float(opts[1])
								
							if abs(float(opts[2]))<=30:
								if abs(monsters[opts[0]]['ycord']+float(opts[2]))>=150:
									monsters[opts[0]]['ycord']=150
								else:
									monsters[opts[0]]['ycord']+=float(opts[2])

							
						print(monsters)
						oscmsg = OSC.OSCMessage()
						oscmsg.setAddress("/"+opts[0])

						oscmsg.append([float(opts[1]),float(opts[2])])
						c.send(oscmsg)

					if "!arrow" in message:
						opts=message.split("arrow")[1]
						opts=opts.strip().split(" ")
						if len(opts)==2:
							if abs(arrow['xcord']+float(opts[0]))>=150:
								arrow['xcord']=150
							else:
								arrow['xcord']+=float(opts[0])
								
							if abs(arrow['ycord']+float(opts[1]))>=150:
								arrow['ycord']=150
							else:
								arrow['ycord']+=float(opts[1])
							
						print(arrow)
						oscmsg = OSC.OSCMessage()
						oscmsg.setAddress("/"+"arrow")
						oscmsg.append([float(opts[0]),float(opts[1])])
						c.send(oscmsg)
					
					if "!poll" in  message and "end" not in message:
						poll={}
						
						print("Asked to start a poll!!")
						
						sendMessage(s, "Asked to start a poll!!", chan)
							
						opts=message.split("poll")[1]
						print(opts)
						opts=opts.split(",")
						for i in opts:
							poll[i.strip()+str(chan)]=0
						print(poll.keys())
						pollStarted=True
					
					
# 					for char in string.punctuation:
# 						message = message.replace(char, ' ')
						
					words=message.split()
					
					if wdf and "A poll has been started" not in message:
						for i in words:
							i=i.replace(cue,'').lower()
							for char in string.punctuation:
								i = i.replace(char, '')
							
							if "!poll end"in message:
								continue
							if i+str(chan) not in poll:
								poll[i+str(chan)]=1
							else:
								poll[i+str(chan)]+=1
					
					
					if "what do you guys think" in message:
						
						print("poll Started!")
						wdf=True
						sendMessage(s, "A poll has been started!", chan)
						cue=message.split("what do you guys think")[1]
						for char in string.punctuation:
								cue = cue.replace(char, '')
						print(cue)
					
					
					if "!vote" in message and pollStarted:
						
						print("A vote came in")
						vote=message.split("vote")[1].strip()
						
						for char in string.punctuation:
							vote = vote.replace(char, '')
						
						print(vote)
						if vote+str(chan) in poll:
							poll[vote+str(chan)]+=1
						else:
							poll[vote+str(chan)]=1
						print(poll)
					if "!poll" in message and "end" in message:
						pollResults="<<< Poll results >>>  ________________"
						vals=-1
						higher=""
						for i in poll:
							pollResults+=" %s-%d ,  "%(i.replace(str(chan),''),poll[i])
							if poll[i]>vals:
								vals=poll[i]
								higher=i.replace(str(chan),'')
							
						print(pollResults)
						pollResults+=" ***** %s ***** %s   wins!   ***** %s *****"%(emo(),higher,emo())
						pollStarted=False	
						wdf=False
						
												
						sendMessage(s,pollResults,chan)
						

					print("%s:%s"%(user,message))
		
				# Survives if there's a message problem
				except Exception as e:
					print "MESSAGE PROBLEM"
					print line
					print e
		
			# Responds to PINGs from twitch so it doesn't get disconnected
			elif "PING" in line:
				try:
					separate = line.split(":", 2)
					s.send("PONG %s\r\n" % separate[1])
					print ("PONG %s\r\n" % separate[1])
					print "I PONGED BACK"
				
				# Survives if there's a ping problem
				except:
					print "PING problem PING problem PING problem"
					print line
		
			# Parses ban messages and writes them to the file
			elif "CLEARCHAT" in line:
				try:
			
					# Gets banned user's name and channel name from a line
					user = getBannedUser(line)
					channelname = getBannedChannelname(line)
				
					# Writes Message ID, channel, user, date/time, and an indicator that it was a ban message.
					#	I use "oghma.ban" because the bot's name is oghma, and I figure it's not a phrase that's
					#	likely to show up in a message so it's easy to search for.
					with open('outputlog.csv', 'ab') as fp:
						ab = csv.writer(fp, delimiter=',');
						data = [id, channelname, user, datetime.now(), "oghma.ban"];
						ab.writerow(data);
			
				# Survives if there's a ban message problem
				except Exception as e:
					print "BAN PROBLEM"
					print line
					print e
				