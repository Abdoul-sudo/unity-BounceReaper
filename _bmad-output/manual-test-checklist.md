# Test Checklist — Bounce Reaper

## Core Loop
- [ ] Viser (click drag) → tirer → balles rebondissent → reviennent en bas
- [ ] Blocs avec HP, disparaissent quand HP = 0
- [ ] Nouvelle rangée de blocs chaque tour
- [ ] Game Over quand un bloc touche le bas

## Balles
- [ ] 1 balle visible en bas au start
- [ ] Dernière balle à revenir = position du prochain tir
- [ ] Pas de bug rebond vertical infini

## Pickups & Currency
- [ ] Blocs verts "+1" dans la grille → donnent +1 balle
- [ ] Shards gagnés en détruisant des blocs
- [ ] Compteur shards en haut à gauche se met à jour

## Upgrades
- [ ] Panel upgrade apparaît entre les tours
- [ ] Damage / Speed / Extra Balls achetables avec shards
- [ ] SKIP ferme le panel et relance la visée

## Save
- [ ] Shards + upgrades persistent après restart Play
