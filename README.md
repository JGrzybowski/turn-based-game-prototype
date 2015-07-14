# A prototype of turn-based game
A turn-based game prototype for the recrutation in CD Project RED

Time limit: 5 days.

Screenshot from the prototype
https://onedrive.live.com/embed?cid=5DB4DC991134C108&resid=5DB4DC991134C108%219289&authkey=ACUSsayV77klXw4

1. Battle grid with buttons to navigate.
2. Light green area is area where we can move.
3. Hourglass buttons allows units to wait.
4. In this place the spells of given unit appear.

Avaliable feature:
- Spells dealing damage, healing, buffing and debuffing
- Meele and ranged attack
- Different damage types
- Vulnerability and resistance on different damage types
- Teleport-like movement (without path finding)
- 6 prefabs of units
- Movement range visualisation

What I would do differently?
- UNIT TESTS :/ (prototyping should be fast, but it would be worth it)
- Units accessed through the hex
- More generic damage typing and calculating resistance
- Separated UI and Battle Engine

Possible ways to improove the prototype:
- Add path finding with A* algorithm
- Option to get close to enemy and attack him in one action
- System could be more based on events, so classes would be more separated
- Terrain events
- Unity Editor extensions for easier designing units and spells
- Grid coloring based on events
- Units coloring (active unit, enemies, allies)

