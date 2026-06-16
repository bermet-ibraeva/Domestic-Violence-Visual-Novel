# When the Lights Go Out

An educational mobile visual novel developed in Unity to raise awareness of domestic violence among teenagers through interactive storytelling and choice-driven gameplay.

## Overview

**When the Lights Go Out** is a narrative-driven mobile application that combines interactive storytelling with educational content. The project was developed as a graduation thesis and aims to help teenagers recognize warning signs of domestic violence, understand available support options, and develop empathy toward affected individuals.

The game follows Ainaz, a teenage girl who gradually becomes aware of domestic violence occurring within her friend Guldana's family. Through meaningful choices and branching storylines, players experience the consequences of different actions and responses in difficult situations.

## Key Features

### Branching Narrative System

* Multiple story paths and endings
* Choice-driven progression
* Dynamic character relationships
* Consequence-based decision making

### Relationship & Trust System

* Ainaz ↔ Guldana trust tracking
* Aida ↔ Jamila trust tracking
* Relationship-dependent story outcomes
* Hidden decision impacts

### Educational Integration

* Interactive learning notes
* Knowledge assessment quizzes
* Unlockable educational content
* Progress-based rewards

### Save & Progression System

* JSON-based save/load architecture
* Persistent player statistics
* Episode progression tracking
* Educational content completion tracking

### Localization

* English
* Russian
* Kyrgyz

### Mobile-Oriented Design

* Android support
* iOS support
* Responsive UI
* Touch-friendly interactions

## Technical Stack

| Technology         | Purpose               |
| ------------------ | --------------------- |
| Unity              | Game Engine           |
| C#                 | Core Development      |
| JSON               | Dialogue & Save Data  |
| TextMeshPro        | UI Text Rendering     |
| Unity UI           | User Interface        |
| Scriptable Objects | Content Configuration |

## System Architecture

The project follows a modular architecture where gameplay systems are separated into independent managers and controllers.

Main systems include:

* Dialogue Controller
* Background Controller
* Audio Manager
* Save System
* Localization Manager
* Notes System
* Quiz System
* Episode Progression Manager
* Feedback Collection Service

## Gameplay Systems

### Dialogue System

The dialogue framework is fully data-driven and loads story content from external JSON files. This allows writers and designers to modify story content without changing source code.

### Choice System

Player decisions trigger effects that modify character relationships and story statistics. These values influence future dialogue, available choices, and ending conditions.

### Statistics System

The game tracks several hidden and visible variables:

* Trust (Ainaz & Guldana)
* Trust (Aida & Jamila)
* Risk Level
* Safety Awareness
* Reward Points

These variables directly affect narrative progression and ending outcomes.

## Educational Goal

Unlike traditional awareness materials, the project uses interactive storytelling to simulate realistic situations and encourage active engagement. Players are not only informed about domestic violence but are required to make decisions within challenging social scenarios.

## Screenshots

Add screenshots here:

* Main Menu
* Dialogue Screen
* Choice System
* Notes Section
* Quiz System
* Episode Summary

## Future Improvements

* Additional story episodes
* Expanded educational materials
* Voice acting support
* Analytics dashboard
* Cloud save functionality
* Additional language support

## Project Outcome

The project was successfully developed and evaluated as a university graduation thesis. Feedback from domain experts, including a crisis center director and child psychologist, was incorporated during development to improve educational accuracy and narrative authenticity.

## Author

**Bermet Ibraev**

Software Engineering Graduate

Unity Developer | Game Development | Human-Computer Interaction
