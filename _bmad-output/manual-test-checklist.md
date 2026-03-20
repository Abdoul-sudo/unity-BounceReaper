# Bounce Reaper — Manual Test Checklist

Run after each major change. Stop Play, do **BounceReaper > Setup > 2 - Reset Scene Only**, Ctrl+S, then Play.

---

## Setup
- [ ] Menu BounceReaper visible dans la barre Unity
- [ ] Setup > 1 crée tous les assets sans erreur
- [ ] Setup > 2 reset la scène sans erreur
- [ ] Aucune erreur dans la Console au démarrage

## Arène & Camera
- [ ] Arène en portrait (plus haut que large)
- [ ] Fond sombre (quasi-noir)
- [ ] Murs gauche, droite, haut visibles
- [ ] Pas de mur en bas (balles tombent)

## Balle — Visée
- [ ] Balle visible en bas au démarrage
- [ ] Click + drag affiche la ligne de visée
- [ ] Ligne de visée limitée aux angles vers le haut
- [ ] Release tire les balles
- [ ] Impossible de tirer vers le bas

## Balle — Physique
- [ ] Balle rebondit sur les murs
- [ ] Balle rebondit sur les blocs
- [ ] Balle ne reste pas bloquée en rebond vertical
- [ ] Balle disparaît quand elle revient en bas
- [ ] Dernière balle définit la position de lancement suivante
- [ ] Une seule balle visible entre les tours

## Blocs / Grille
- [ ] 3 rangées de blocs au démarrage
- [ ] Blocs affichent leur HP
- [ ] HP diminue quand la balle frappe
- [ ] Bloc disparaît quand HP = 0
- [ ] Couleur des blocs varie selon les HP
- [ ] Nouvelle rangée spawn après chaque tour
- [ ] Blocs descendent d'une ligne à chaque tour

## Pickups +1 Balle
- [ ] Blocs verts "+1" apparaissent (~50% des rows)
- [ ] Taille similaire aux blocs normaux
- [ ] Affichent "+1" au lieu d'un HP
- [ ] Quand détruits → ball count augmente
- [ ] HUD "xN" se met à jour

## Currency (Shards)
- [ ] Shards gagnés quand un bloc est détruit
- [ ] HUD shards se met à jour en temps réel
- [ ] Log "[Currency] +X shards this turn" en fin de tour
- [ ] Shards sauvegardés (restart Play → shards persistent)

## Upgrades
- [ ] Panel upgrade apparaît entre les tours (après retour des balles)
- [ ] 3 boutons : Damage (rouge), Speed (bleu), Extra Balls (vert)
- [ ] Bouton SKIP ferme le panel et lance la visée
- [ ] Acheter un upgrade déduit les shards
- [ ] Bouton grisé si pas assez de shards
- [ ] Coût augmente après chaque achat
- [ ] Upgrade Damage → blocs meurent plus vite
- [ ] Upgrade Speed → balles plus rapides
- [ ] Upgrade Extra Balls → plus de balles tirées au prochain tour
- [ ] Upgrades sauvegardés entre sessions

## Game Over
- [ ] "Block reached bottom" déclenche le game over
- [ ] Écran Game Over apparaît (fond sombre)
- [ ] Affiche le wave number + shards
- [ ] Bouton RESTART relance la scène
- [ ] Après game over, impossible de viser/tirer

## HUD
- [ ] Barre sombre en haut avec shards (jaune) et wave (blanc)
- [ ] Ball count "xN" visible en bas
- [ ] Wave number se met à jour à chaque nouvelle row

## Save System
- [ ] Log "[Save] Saved to..." après chaque tour
- [ ] Shards persistent entre Play sessions
- [ ] Upgrades persistent entre Play sessions
