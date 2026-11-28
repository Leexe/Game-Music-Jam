EXTERNAL HasItem(itemId)
EXTERNAL HasInteracted(objectId)
EXTERNAL RecordInteraction(objectId)
EXTERNAL HasPhotographedAllEvidence()

VAR learned_safe_combo = false
VAR heard_lawyer_knife_lie = false
VAR heard_mitch_saw_lawyer = false
VAR talked_to_laila = false
VAR talked_to_naismith = false
VAR talked_to_mitch = false
VAR talked_to_chief_reveal = false

-> new_interview

INCLUDE items.ink

=== function HasItem(itemId) ===
~ return false

=== function HasInteracted(objectId) ===
~ return false

=== function RecordInteraction(objectId) ===
~ return

=== function HasPhotographedAllEvidence() ===
~ return false

=== function CanCallChief() ===
~ return talked_to_laila and talked_to_naismith and talked_to_mitch and HasPhotographedAllEvidence()

=== new_interview === 

Who to call? 

+ [Laila, the assistant.] 
-> asst_interview 

+ [Naismith, the lawyer.] 
-> lawyer_interview

+ [Mitch, the chauffeur.] 
-> driver_greet

+ {CanCallChief() and not talked_to_chief_reveal} [The chief. I need to speak to him.] 
-> chief_reveal 

+ {CanCallChief() and talked_to_chief_reveal} [The chief. I need to speak to him.] 
-> chief_convince 

+ [Put down the phone.]
-> END

=== new_interview_1 === 

Who to call? 

+ [Laila, the assistant.] 
-> asst_interview 

+ [Naismith, the lawyer.] 
-> lawyer_interview

+ [Mitch, the chauffeur.] 
-> driver_greet

+ {CanCallChief() and not talked_to_chief_reveal} [The chief. I need to speak to him.]  
-> chief_reveal 

+ {CanCallChief() and talked_to_chief_reveal} [The chief. I need to speak to him.]  
-> chief_convince 

+ [Put down the phone.]
-> END

/* 

Beginning of Lawyer Interview 

*/ 

=== lawyer_interview ===
~ talked_to_naismith = true
PROTAG: What do you know about... #call_Naismith

+ ...the missing car? #call_Naismith
-> LAWYER_CAR
  
+ {HasInteracted("safe")} ...the safe? #call_Naismith
 -> LAWYER_SAFE

+ {HasItem("knife")} ...the knife? #call_Naismith
  -> LAWYER_KNIFE

+ ...her appointment with you? #call_Naismith
  -> LAWYER_APPT

+ You know what, nevermind. Thanks for all your help. #call_Naismith
-> new_interview_1

=== LAWYER_CAR === 
NAISMITH: Mina's car is missing? I never did like that chauffeur of hers. Mitch. He has a criminal record, you know. #call_Naismith
PROTAG: Interesting. Thank you. #call_Naismith

->lawyer_interview

=== LAWYER_SAFE === 
NAISMITH: I imagine she kept important documents in it. I've never been to her house; I wouldn't know. She insisted on having all of our meetings at my office. #call_Naismith
PROTAG: You wanted to have them somewhere else? #call_Naismith
NAISMITH: We often discussed the state of her ancestral home. I suggested meeting her there to have it inspected. She seemed... against that idea. #call_Naismith
PROTAG: Noted. Thank you. #call_Naismith
{learned_safe_combo:
    PROTAG: Laila mentioned her will was inside. Do you handle her estate? #call_Naismith
    NAISMITH: Only what is strictly necessary, detective. #call_Naismith
}
->lawyer_interview

=== LAWYER_KNIFE === 
~ heard_lawyer_knife_lie = true
NAISMITH: Knife? I wouldn't touch her knives with a 20-foot pole. They're not even made of stainless steel. #call_Naismith
{HasItem("knife"):
    PROTAG: Funny you know what her kitchen knives are made of. #call_Naismith
}
->lawyer_interview

=== LAWYER_APPT === 
NAISMITH: We had an appointment yesterday, that's true. The nature of what we discussed is private due to attorney-client privilege, I'm sure you can understand. #call_Naismith
PROTAG: Of course. But you can verify you last saw her yesterday afternoon? #call_Naismith
NAISMITH: Yes. #call_Naismith
NAISMITH: Has Ms. Wainwright gone missing? #call_Naismith
NAISMITH: No, it hasn't even been 24 hours. The department wouldn't open a missing persons case. #call_Naismith
NAISMITH: What's going on here? #call_Naismith
PROTAG: That's what we're all trying to figure out, Mr. Naismith. Thank you. #call_Naismith
->lawyer_interview

/* 

Beginning of assistant interview 

*/

=== asst_interview === 
~ talked_to_laila = true

PROTAG: What do you know about... #call_Laila
+ ...Lumina's appointment with her lawyer? #call_Laila
-> asst_appt 

+ The missing car? #call_Laila
-> asst_car 

+ {HasInteracted("safe")} The safe? #call_Laila
-> asst_safe

+ {HasItem("knife")} the knife? #call_Laila
-> asst_knife

+ You know what, nevermind. Thanks for all your help. #call_Laila
-> new_interview_1 

=== asst_appt === 
LAILA: I didn’t know she had an appointment with Ned. When was this? #call_Laila
PROTAG: Yesterday. #call_Laila
LAILA: Strange. She usually tells me everything. She's not supposed to keep things from me. I'm her favorite. I'm her favorite. I'm her favori- #call_Laila
PROTAG: Thank you for your concern, Ms. Oscura. #call_Laila
-> asst_interview 
=== asst_car === 
LAILA: She left yesterday afternoon with the car. She came back without it. You’d have to ask Mitch, her driver. I wish she wouldn't go places without me. She looked all torn up. #call_Laila
-> asst_interview 
=== asst_safe === 
{learned_safe_combo:
    LAILA: Like I said, the combination is 8-8-6-4. Her will and the deed are in there. #call_Laila
- else:
    ~ learned_safe_combo = true
    LAILA: She keeps everything important in there. #call_Laila

    PROTAG: Like what? #call_Laila

    LAILA: Her will, deed to the house, those kinds of things. #call_Laila

    PROTAG: Would you happen to know the combination? #call_Laila

    LAILA: It’s 8-8-6-4. I love watching her slender fingers tapping those keys. #call_Laila
}
-> asst_interview 
=== asst_knife === 
LAILA: I saw that this morning. #call_Laila

PROTAG: You were here this morning? #call_Laila

LAILA: Yes, I was reporting to work. I was eating breakfast. I came in through the back door. #call_Laila

PROTAG: And? #call_Laila

LAILA: And I noticed the knife was missing. Then the chief found me and led me out. Without letting me talk to Lumina! I talk to her every day. When can I see her? She didn't fire me, did she? #call_Laila

PROTAG: Soon, Ms. Oscura. You can see her soon. #call_Laila
-> asst_interview 

/*

Beginning of driver interview 

*/ 

=== driver_greet === 
~ talked_to_mitch = true
PROTAG: Hello, Mr. Ackleman. #call_Mitch
-> driver_interview 

=== driver_interview === 
PROTAG: What do you know about...  #call_Mitch

+ ...the missing car? #call_Mitch
-> dr_car
  
+ {HasInteracted("safe")} ...the safe? #call_Mitch
 -> dr_safe

+ {HasItem("knife")} ...the knife? #call_Mitch
  -> dr_knife

+ ...her appointment with her lawyer? #call_Mitch
  -> dr_appt

+ You know what, nevermind. Thanks for all your help. #call_Mitch
-> new_interview_1

=== dr_car === 
MITCH: Is that what this is about? I swear I didn’t have anything to do with it.  #call_Mitch
PROTAG: Do with what? #call_Mitch
MITCH: Totalling the car.  #call_Mitch
PROTAG: Who totalled Ms. Wainwright’s car? #call_Mitch
MITCH: Well, she did. After she left yesterday afternoon. She came home and said it was a wreck and I may as well go home.  #call_Mitch
PROTAG: Then?  #call_Mitch
MITCH: I left. In my car. #call_Mitch
PROTAG: Noted. Thank you.  #call_Mitch

-> driver_interview 

=== dr_appt ===
~ heard_mitch_saw_lawyer = true
MITCH: Is that where she went yesterday?  #call_Mitch
PROTAG: That does seem to be the case, Mr. Ackleman.  #call_Mitch
MITCH: I never trusted him. I met the guy once when he came over. Real piece of work. Practically drooled all over Lumina, too. She never paid him any attention, though, beyond what was polite. I'd almost think she didn't like the guy.  #call_Mitch
-> driver_interview 

=== dr_safe === 
MITCH: Oh, I don’t know. She spent a lot of time in her office. Ask Laila. That girl watches her like a hawk.  #call_Mitch
PROTAG: Thank you. #call_Mitch
-> driver_interview 

=== dr_knife === 
MITCH: Lumina-- sorry, Ms. Wainwright-- had these cheap knives she got from somewhere. A lot of them. She said they weren't really for cooking.  #call_Mitch
PROTAG: Then what were they for? #call_Mitch
MITCH: Self defense? She always did think she was like a girl from one of the movies. #call_Mitch
MITCH: But she seemed plenty strong. One time, the car got stuck by the side of the road, and she pushed it all the way to the gas station without letting me help at all. Went pretty damn fast, too. If you'll excuse my language, Detective.  #call_Mitch
PROTAG: Huh. Thank you for that information.  #call_Mitch
-> driver_interview 


=== chief_reveal === 
~ talked_to_chief_reveal = true
PROTAG: Hi, Chief. Something about this case seems... strange. #call_Chief
CHIEF: How so? #call_Chief
PROTAG: No one can tell me where Lumina's money comes from. She collects knives. There's a weird chemical smell coming from one of the rooms. Was she... involved in something? #call_Chief
CHIEF: (sigh) #call_Chief
CHIEF: Detective, there's a lot going on in this town you haven't gotten... acquainted with quite yet. Lumina Wainwright was a part of that. #call_Chief
PROTAG: What do you mean? #call_Chief
CHIEF: Well, son, she wasn't quite... human. A fair number of people here aren't either. #call_Chief
CHIEF: That's part of why I wanted you on the force. #call_Chief
PROTAG: You what? #call_Chief
CHIEF: To get an outside perspective. From a more... mundane set of eyes. #call_Chief
PROTAG: I thought I was seeing things. #call_Chief
CHIEF: You are, son. But they're real.  #call_Chief
-> new_interview_1 

/* 
Convince the chief to arrest someone
*/ 

=== chief_convince === 
PROTAG: Hi, Chief. I think I found our suspect. #call_Chief 
CHIEF: Lay it out for me.  #call_Chief 
PROTAG: I think it's the... #call_Chief 
+ The lawyer.  #call_Chief 
-> law_ev
+ The assistant.  #call_Chief 
-> asst_ev
+ The chauffeur.  #call_Chief 
-> dr_ev

=== law_ev ===
CHIEF: Why do you think it's the lawyer? #call_Chief 
{heard_lawyer_knife_lie and heard_mitch_saw_lawyer:
    PROTAG: He had a vested interest in Ms. Wainwright's affairs. She was run off the road after leaving his office yesterday, after having a secret appointment she didn't want anyone to know about. And he lied to me.  #call_Chief 
    CHIEF: He lied? #call_Chief 
    PROTAG: He said he's never been here before. But he commented on the quality of her kitchen knives. And the chauffeur said he met the guy at the house before. #call_Chief
    CHIEF: Smart eye, detective. What's his motive? #call_Chief 
    PROTAG: I think he was interested in her and she rejected him. I found a love letter in the coat she wore yesterday. And now that she's gone, he's the one in charge of her money.  #call_Chief 
    CHIEF: Rejection can lead a man to do crazy things. Especially when he decides if he can't take her money one way, he'll do it the other. Alright, detective. I'll bring him in. Do you have any evidence? #call_Chief 
    PROTAG: That camera you gave me takes some very interesting photographs.  #call_Chief 
    CHIEF: Yes, yes it does.  #call_Chief 
    CHIEF: Ned's one of those... others I was telling you about. Now that I think about it, it could have only been him. #call_Chief  
    -> arrest_sequence 
- else:
    PROTAG: He had a secret appointment with her yesterday and stands to benefit from her estate. #call_Chief
    CHIEF: That's circumstantial at best. You haven't caught him in a lie or tied him to the house yet. Keep digging, detective. #call_Chief
    -> END
}

=== asst_ev === 
CHIEF: Why do you think it's the assistant? #call_Chief 
PROTAG: She was here this morning when you got here. She claimed not to know about an appointment Ms. Wainwright had, even though she must've seen her leave. #call_Chief
CHIEF: A woman showing up to do her job is hardly damning.  #call_Chief 
{HasInteracted("safe"):
    PROTAG: How do you explain her having the combination to the safe? #call_Chief 
    CHIEF: She works for the woman.  #call_Chief 
    PROTAG: More than works for. She seems totally obsessed. I found a love letter in Lumina's coat pocket. Unopened. They could've had an argument about it that spiralled out of control.  #call_Chief 
    CHIEF: Laila Oscura is not of the supernatural element. She couldn't have overpowered Lumina. And "could've" doesn't suggest you have any evidence. #call_Chief
    CHIEF: Find someone else, detective.  #call_Chief 
- else:
    CHIEF: Call me back when you have real evidence against her, detective. #call_Chief
}
-> END

=== dr_ev === 
CHIEF: Why do you think it's the chauffeur? #call_Chief 
PROTAG: He has no alibi for last night and claims to have let Ms. Wainwright drive herself to an appointment, after which she crashed her car. Hardly doing his job. And according to Mr. Naismith, he has a criminal record.  #call_Chief 
CHIEF: No one could have an alibi given the absolute torrential downpour we all suffered. And ol' Mitch just has a charge for drunken and disorderly for peeing on a fire hydrant a couple years back.  #call_Chief 
CHIEF: Besides, he's one of the most ordinary men I've ever met. He couldn't have beaten Lumina in an arm-wrestling contest. No way he could've bested her in a knife fight.  #call_Chief 
CHIEF: Find someone else, detective.  #call_Chief 
-> END 

=== arrest_sequence === 

PROTAG: Ned Naismith, despite his many years in the profession, did not practice in criminal court and got the full sentence for first-degree murder with the help of those photographs. And a little provoking to show his true colors to our all-human jury, of course. 
-> END 

