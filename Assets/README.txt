README – SpeakUp! VR Generative AI Application
🔹 Project Overview

SpeakUp! is a VR educational application designed to help students practice public speaking and social interactions.
It integrates Unity with OpenAI services (Whisper + GPT + TTS) to provide AI-driven NPC conversations, feedback systems, and multi-agent interactions.

🔹 Project Information

Developer: Eddie Ooi Wei Kit (Final Year Project, TARUMT)

Unity Version: 6000.0.48f1 LTS

Programming Languages: C#, Unity Engine, OpenAI API

External Services: OpenAI Whisper (speech-to-text), GPT (NPC brain), OpenAI TTS

🔹 Scenes Overview

Main Menu – Load Game, Settings, About Us, Exit

Hallway Scene – Non-GPT NPCs, warm-up area before presentation

Classroom Scene – Presentation mode with feedback system + virtual assistant coach

Public Speaking Venue – Stage presentation gameplay (same system as classroom)

Golden Dragon Kopitiam – Multi-agent NPC hangout, GPT + non-GPT NPCs interacting

🔹 Setup Guide

Install Unity

Clone/Unzip project folder

Open Unity Hub → Add Project → Select Folder

Restore Packages

Unity will auto-download XR Toolkit, OpenAI SDK, TextMeshPro, etc.

API Key Setup

Create folder: C:\Users\<username>\.openai\

Add auth.json with:

{
  "api_key": "your_openai_api_key_here"
}

🔹 Gameplay Instructions

Movement: Left joystick / holding shift + WASD (for PC test)

Look Around: VR headset / Mouse (PC test)

Interactions: Trigger button (VR controller) / Left click

Presentation Flow:

Enter classroom → Start recording → Whisper transcribes speech

GPT NPC responds (with TTS + lipsync)

Feedback system analyzes metrics (duration, WPM, filler words)

Multi-Agent Hangout: NPCs talk with each other + user in Kopitiam scene

For questions about this project:
Name: Eddie Ooi Wei Kit
LinkedIn: www.linkedin.com/in/eddieooi

Email: ooiwk-wm23@student.tarc.edu.my
