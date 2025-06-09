# AI Driven VR Reverse Dodgeball

*Thibo Maes (2ITSOF3b, s150673)*

## Inleiding

Dit document beschrijft het opzetten van een Unity-project waar een AI-agent leert om ballen te vangen in een virtuele omgeving. We combineren de kracht van machine learning met de immersie van virtual reality om een dynamische, interactieve simulatie te maken. 

Hiermee willen we niet een complexe game maken, maar wel een simpel proof-of-concept waar de kract van machine learning centraal staat, aan de hand van de verschillende stappen die je zal doorlopen bij het trainen. Ook worden enkele gedragspatronen van het leerproces besproken die jij ook misschien zal opmerken.

## Methoden

### Installatie

| Package naam | | Versie |
| :- | :- | :- |
| ML Agents | `com.unity.ml-agents` | 2.0.1 |
| MockHMD XR Plugin | `com.unity.xr.mock-hmd` | 1.4.0-preview.2 |
| OpenXR Plugin | `com.unity.xr.openxr` | 1.13.2 |
| XR Core Utilities | `com.unity.xr.core-utils` | 2.3.0 |
| XR Interaction Toolkit | `com.unity.xr.interaction.toolkit` | 2.6.4 |
| XR Legacy Input Helpers | `com.unity.xr.legacyinputhelpers` | 2.1.11 |
| XR Plugin Management | `com.unity.xr.management` | 4.5.1 |

### Verloop simulatie

- De agent en de speler spawnen aan weerszijden van de vierkante arena.
- De speler gebruikt zijn rechtercontroller om te richten.
- De speler klikt op de hoofdtrigger om een bal te schieten.
- De agent doet zijn algoritme werken om de bal in zijn pad te onderscheppen.
- Na 10 seconden, of als de bal is gevangen, wordt de arena naar zijn originele staat hersteld. De speler mag in zijn positie blijven staan.

### Agent observaties, mogelijke acties, beloningen

Observaties:

- Eigen positie
- Eigen rotatie om de Y-as
- Verschil tussen eigen positie en positie van de bal
- Snelheid van eigen rigidbody
- Snelheid van rigidbody bal
- De volgende observaties zijn deel van een geschrapt spelelement:
  - Afstand tot no-go zone
  - Verschil tussen eigen positie en dichtste punt tot no-go zone
- Totaal: 17 vector observaties
- Geen raycast of camera sensor

Mogelijke acties:

- 2 continue acties:
  - Voorwaards/achterwaards bewegen
  - Draaien rond de Y-as
- Geen discrete acties

Beloningen:

| Voorwaarde | Beloning | Beëindigt episode |
| :- | :- | :- |
| Agent raakt bal aan | `10` | Ja |
| Agent raakt no-go zone aan | `-3` | Ja |
| Agent valt van het platform af | `-10` | Ja |
| Bal valt van het platform af | `-5` | Ja |
| Bal raakt de grond aan | `-1` | Nee |
| Bewegen en/of draaien | `0.01` * `afstand` * `deltaTime` | Nee |

### Afwijkingen van de one-pager

Door een gebrek aan tijd en onderschatte complexiteit heb ik het idee van *no-go zones* moeten schrappen. Het bleek dat de ML-agent er niet omheen kon in de stages van het project waar ik trachtte het aan te leren, dus had ik besloten om het te houden bij enkel het reverse dodgeball aspect.

Het VR-gedeelte is ook niet perfect gelukt: door een gebrek aan ervaring met VR-gerelateerd materiaal en geen VR-bril om dit zelf te testen, moest alles getest worden aan de hand van een Unity-aangeleverde simulator. Deze implementatie verschilt waarschijnlijk van de ingeleverde, ongeteste `.apk`-file waarvan de werking niet is gegarandeerd. Ook is het afstemmen van de agent buiten de training episode-cycle niet gelukt, waardoor de agent soms onvoorspelbaar gedrag vertoont als er geen of meerdere ballen aanwezig zijn op het veld.

## Resultaten

Trainingsessies worden incrementeel genummerd, waar elke opeenvolgende nummer betekent dat deze sessie de vorige sessie als checkpoint heeft gebruikt. Bij uitzondering is `01-03-01` voortgebouwd op `01-03`.

| Overzicht | Legende |
| :- | :- |
| ![All](/graphs/all.png) | ![Legend](/graphs/legend.png) |

### Training 01-01

Parameters: horizontale afwijking `[-10, 10]`°, verticale afwijking `0`°.

![01-01](/graphs/01-01.png)

### Training 01-02

Parameters: horizontale afwijking `[-20, 20]`°, verticale afwijking `[-5, 15]`°.

![01-02](/graphs/01-02.png)

### Training 01-03

Parameters: horizontale afwijking `[-30, 30]`°, verticale afwijking `[-5, 30]`°.

![01-03](/graphs/01-03.png)

### Training 01-04

![01-04](/graphs/01-04.png)

Aanpassen afstraffingsschema voor beweging: van `0.01` voor elke soort beweging naar `0.01` voorwaards, `0.02` rotatie, `0.04` achterwaards.

### Training 01-03-01

Aanpassen van X-coordinaat spawnlocatie: afwijking van `[-4, 4]` units.

![01-03-01](/graphs/01-03-01.png)

### Training 01-03-02

Vroegtijdig beeindigd: hier wilde ik no-go zones aanleren.

### Opvallende waarnemingen

Tijdens de verschillende trainingssessies vertoonde de agent een opmerkelijk consistente gedragspatroon dat werd vastgesteld in de eerste trainingssessie.

- Bij de eerste trainingssessie (`01-01`) ontwikkelde de agent een specifieke strategie: hij week eerst consequent uit naar rechts, om vervolgens in een achterwaartse cirkelbeweging te draaien en zo op het pad van de bal te komen.
- De tweede trainingssessie (`01-02`), die voortbouwde op de eerste, resulteerde in een meer verfijnde versie van ditzelfde gedrag. De agent leek zelfverzekerder in zijn bewegingen: het 'schudden' of trillen bij het stilstaan was minder merkbaar en de draaibewegingen waren minder hevig.
- De derde trainingssessie (`01-03`) was opmerkelijk omdat deze twee keer langer duurde dan de voorgaande sessies, maar resulteerde in een lagere piek voor de gemiddelde beloning. Dit suggereert dat de toegenomen moeilijkheid (maximaal haalbare afwijking in het schieten) het leerproces aanzienlijk vertraagde.
- Het aanpassen van het afstraffingsschema in de vierde sessie (`01-04`) was bedoeld om het eerder vastgestelde gedragspatroon weg te werkem, maar het veranderde het gedrag van de agent niet op een direct merkbare manier.
- De vijfde sessie (`01-03-01`), waar de spawnlocatie van de agent werd gevarieerd, toonde een iets lagere cumulatieve beloning. Dit is te verwachten, aangezien een willekeurige startpositie het risico vergroot dat de bal buiten bereik of van het platform belandt. Toch bleef de agent verbeteren.

## Conclusie

Dit project had als doel een AI-agent via reinforcement learning autonoom te leren om ballen te vangen in een virtuele omgeving. Uit de resultaten blijkt dat de agent een strategie heeft kunnen ontwikkelen om de bal te vangen. Het gedrag vertoont toch onverwachte patronen en zorgt niet consistent voor succes.

De resultaten, omgezet naar een persoonlijke visie, betekenen dat de aanzet naar complex gedrag aanleren vrij simpel is, maar het uitwerken naar dat wat je exact wilt krijgen, toch een ander gegeven is. In de toekomst tracht ik meer complexe mechanica vroeger aan te leren, en afwijkingen in verwacht gedrag vroeger aan te pakken door middel van een aanpassing van het afstraffingsschema. Het vroegtijdig afstemmen naar de meest efficiënte gedragspatronen kan zorgen voor een efficiënter en gemakkelijker leerproces, wat het aanleren van ingewikkeldere situaties simpeler maakt.
