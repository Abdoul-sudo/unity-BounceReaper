---
title: 'Game Brainstorming Session'
date: '2026-03-08'
author: 'Abdoul'
version: '1.0'
stepsCompleted: [1, 2, 3, 4]
status: 'complete'
---

# Session de Brainstorming de Jeu

## Infos de Session

- **Date:** 2026-03-08
- **Facilitateur:** Agent Game Designer
- **Participant:** Abdoul

---

## Approche de Brainstorming

**Mode Sélectionné :** Guidé - Exploration technique par technique

**Techniques Disponibles :**
- MDA Framework Exploration
- Core Loop Brainstorming
- Player Fantasy Mining
- Genre Mashup
- Reward Schedule Architecture
- Progression Curve Sculpting
- Meta-Game Layer Design
- Verbs Before Nouns
- Constraint-Based Creativity
- Remix an Existing Game

**Domaines de Focus :**
- Boucle de gameplay centrale (idle + tap)
- Systèmes de progression et rétention
- Monétisation (ads, IAP, monnaie virtuelle)
- Expérience joueur mobile 2D
- Scope réalisable en 1 mois

---

## Ideas Generated

---

### [Concept #1] : Necromancer's Ascent (À développer plus tard)

**Status :** Sauvegardé pour développement futur (scope ~1 mois)

**Core Fantasy :**
Un squelette nécromancien oublié dans un donjon veut se hisser au sommet, vaincre le Roi Démon et gagner sa réincarnation en humain.

**Genre :** Idle side-scroll 2D pixel art avec armée

**Core Loop :**
- Le nécromancien avance en side-scroll automatiquement
- Son armée de morts-vivants combat les ennemis en auto
- Le joueur lance des sorts actifs (3 slots, cooldowns)
- Battre un type d'ennemi = débloquer son "âme" → recrutement possible
- Les ennemis recrutés subissent un palette swap spectral (bleu/noir)

**Système de Troupes :**
- 5 rôles : Tank, DPS Mêlée, DPS Range, Support, AoE
- ~15-20 monstres (3-4 par rôle)
- Chaque troupe : 1 skill passif + 1 skill actif
- Système d'étoiles ★1 → ★5
- Slots limités (5 → 15) → force les choix de composition
- Synergies organiques via interactions de skills (pas de bonus forcés)

**Gear du Nécromancien :**
- 4 slots : Bâton, Armure, Anneau x2, Amulette
- Raretés : Commun → Rare → Épique → Légendaire → Mythique
- Fusion : 3 identiques = niveau supérieur
- Le gear crée des builds (Freeze, Bleed, Rush, etc.)

**Sorts Actifs (3 slots sur 6-8 disponibles) :**
- Drain d'Âme (dégâts + heal, 8s)
- Nova Spectrale (AoE, 12s)
- Rage des Morts (buff armée, 15s)
- Mur d'Ossements (bouclier armée, 10s)
- Invocation Forcée (convertit 1 ennemi, 20s)
- Malédiction (ennemi prend 2x dégâts, 10s)

**Progression :**
- Stages linéaires, boss mineur /10 stages, boss MAJEUR /50 stages
- Boss majeurs avec petits dialogues narratifs
- Prestige avec arbre d'upgrades permanents (branches Armée/Sorts/Loot)
- Prestige narratif : le Roi Démon triche → recommencer plus fort
- Dialogues qui changent post-prestige ("Encore toi ?")

**Idle / AFK :**
- L'armée farm le dernier stage battu en absence
- Cap 8-12h, pub pour doubler les récompenses

**Monétisation :**
- Or (soft) + Gemmes (hard)
- Ads rewarded uniquement (doubler idle, revive, coffre bonus, boost x2)
- IAP : Remove Ads à vie + 20 gems/jour, Starter Pack, Packs de gemmes
- Pacte du Démon (battle pass mensuel) : track gratuit + track payant

**Art Direction :**
- Pixel art 16-bit, palette sombre (noir, violet, bleu, cyan/vert néon)
- Ennemis vivants = couleurs chaudes / Troupes nécro = palette swap froid
- Palette swap par shader (pas besoin de doubler les sprites)

**Lore — Boss Majeurs :**
- Stage 50 : Gardien du Cimetière
- Stage 100 : Chevalier Déchu
- Stage 150 : Sorcière des Marais
- Stage 200 : Dragon Spectral
- Stage 250 : Général de l'Armée Morte
- Stage 500 : Roi Démon (boss final)

---

### [Concept #2] : Bounce Reaper (Concept actif — à développer en premier)

**Status :** Concept validé, prêt pour pré-production
**Plateforme :** Mobile (iOS/Android)
**Engine :** Unity
**Genre :** Idle Bouncer + Combat, mobile 2D
**Scope :** ~1 semaine

**Pitch :**
Tes balles rebondissent et massacrent tout. Achète plus de balles. Upgrade tes balles. Regarde le chaos. Bats des boss. Prestige. Recommence plus fort.

**Core Loop :**
```
Balles rebondissent dans l'arène
  → Touchent des ennemis → Les tuent
    → Drop or + XP + loot
      → Upgrade balles / achète plus de balles
        → Ennemis plus forts (vague suivante)
          → Boucle
```

**Système de Balles :**
| Balle | Effet | Obtention |
|---|---|---|
| Basique | Dégâts normaux | Début du jeu |
| Feu | Dégâts + brûlure DoT | Achat or |
| Glace | Dégâts + slow ennemi | Achat or |
| Poison | Dégâts faibles + poison AoE | Achat or |
| Spectrale | Traverse les ennemis (pierce) | Gemmes |
| Explosive | Explose au contact (AoE) | Gemmes / rare drop |

- Chaque balle upgradeable : dégâts, vitesse, taille
- Le joueur commence avec 1 balle, peut en avoir 20-30

**Upgrades Or (soft currency) :**
- Nombre de balles (+1)
- Dégâts des balles
- Vitesse des balles
- Taille des balles
- Or par kill
- Idle earnings

**Upgrades Gemmes (hard currency) :**
- Types de balles spéciales
- Slots de sorts supplémentaires
- Skins d'arène

**Sorts Actifs (3 slots) :**
| Sort | Cooldown | Effet |
|---|---|---|
| Multiball | 30s | x3 balles pendant 10s |
| Gravity Well | 20s | Balles convergent puis explosent |
| Berserk | 25s | Vitesse x2 pendant 8s |
| Shield Break | 15s | Ignore armure 5s |
| Magnet | 20s | Attire le loot au centre |

**Ennemis :**
| Ennemi | Comportement | HP |
|---|---|---|
| Blob | Statique | Faible |
| Bat | Se déplace lentement | Faible |
| Skeleton | Statique, plus de HP | Moyen |
| Slime Boss | Split en 2 quand tué | Élevé |
| Shield Knight | Bloque les balles d'un côté | Moyen |
| Demon (Boss) | Se déplace, détruit tes balles temporairement | Très élevé |

**Progression :**
- Vagues infinies (1, 2, 3... 1000+)
- Boss tous les 10 vagues
- Prestige : reset vagues, gagne "Âmes" (monnaie prestige)
- Arbre de prestige : bonus permanents (+dégâts, +1 balle au départ, etc.)

**Idle / AFK :**
- Les balles continuent de farmer la dernière vague battue
- Cap 8h, pub pour doubler en revenant

**Cosmétiques avec Boosts :**
| Skin | Boost | Obtention |
|---|---|---|
| Arène Infernale | +10% dégâts feu | Battle Pass |
| Arène Glaciale | +5% chance slow | Gems |
| Balles Néon | +8% vitesse balle | Gems |
| Balles Plasma | +12% dégâts AoE | IAP exclusif |
| Arène Dorée | +15% or par kill | Remove Ads pack |
| Arène Void | +5% chance loot rare | Event limité |

**Monétisation :** Voir document détaillé `monetisation-bounce-reaper.md`

**Art Direction :**
- Style géométrique néon (fond sombre, formes lumineuses, traînées)
- Balles = cercles lumineux avec glow/bloom
- Ennemis = formes géométriques colorées
- Effets de particules pour les traînées et explosions
- All In 1 Sprite Shader pour les effets visuels

**Multilingue :**
- Semaine 1 : Anglais + Français
- Post-lancement : Espagnol, Portugais-Brésil, Japonais, Coréen
- Unity Localization Package
- Peu de texte dans le jeu (avantage du style géométrique)

**Onboarding (30 sec max) :**
1. "Tap to launch!" → Balle part
2. Balle tue un ennemi → "Enemies drop gold"
3. Flèche vers upgrade → "Upgrade your ball!"
4. Flèche vers +1 ball → "More balls = more chaos!"
5. "You're ready. Destroy everything."
- Premier boss (vague 10) facile → victoire garantie
- Sorts et prestige se débloquent avec mini-popups contextuels

**Rétention :**
- Daily login rewards (cycle 7 jours, rewards croissantes)
- Streak system (3/7/14/30 jours → bonus croissants, reset si manqué)
- 3 quêtes quotidiennes → bonus coffre + gems
- Roue de la fortune (1 spin gratuit/jour, pub pour re-spin, jackpot rare)
- Notifications push (max 2-3/jour : idle plein, coffre prêt, offre weekend)

**Events (post-lancement) :**
- Invasion Boss hebdomadaire (boss communautaire 48h)
- Arène Chaos weekend (règles spéciales, classement)
- Défi quotidien (compteur simple)
- Bouncer Frenzy mensuel (60s, score, classement mondial)

**Sound Design :**
- 2 tracks musique (ambiance électro/synthwave + boss fight intense)
- ~15 SFX : rebond (pling cristallin, pitch variable), kill (pop), coin, sort, boss, UI
- Ultimate Game Music Collection pour la musique
- freesound.org / Unity Asset Store pour les SFX

**Assets Unity :**
- All In 1 Sprite Shader → glow/néon sur balles et ennemis
- DOTween Pro → animations UI, juice, screen shake
- Hot Reload → itération rapide
- Text Animator for Unity → textes de dégâts animés, titres
- Ultimate Game Music Collection → musique
- vHierarchy 2.0 → organisation projet
- Feel → pas nécessaire semaine 1, optionnel v2

---

## Themes et Patterns

- **Satisfaction visuelle** : le chaos de multiples balles à l'écran est le hook principal
- **Montée en puissance** : 1 balle → 30 balles = power fantasy progression
- **Frustration contrôlée** : les boss créent des murs → monétisation naturelle
- **Rétention multi-couche** : idle + daily + streak + events
- **Monétisation non-agressive** : ads rewarded > interstitielles, offres contextuelles

## Prochaines Étapes

1. Setup projet Unity (2D Mobile template)
2. Créer le Game Brief détaillé
3. Créer le GDD complet
4. Implémenter le core loop (balles + physique + ennemis)
5. Ajouter upgrades + progression
6. Monétisation (ads + IAP)
7. Polish (SFX, VFX, UI)
8. Localisation EN + FR
9. Build mobile + test
10. Publication

---
