# Quartoto

L’objectif de ce projet est la programmation du jeu de Quarto en C#.
Règles du jeu
Quarto est un jeu de société édité par la société [Gigamic](https://www.gigamic.com). L'objectif du jeu est d’aligner quatre pièces ayant au moins un point commun entre elles. Mais chaque joueur ne joue pas ce qu'il veut, c’est son adversaire qui choisit pour lui.

Vous trouverez plus d’informations sur la [page Wikipedia](https://fr.wikipedia.org/wiki/Quarto) et dans les [règles du jeu](https://www.gigamic.com/files/catalog/products/rules/quarto_rule-fr.pdf).
Fonctionnalités
Le programme réalisé permet à un joueur humain d’affronter l’ordinateur selon les règles standard du jeu.

---

## Tout d'abord, lancer le programme...

![Menu 2 Joueurs](https://raw.githubusercontent.com/PictElm/Quartoto/master/images/menu_2_players.png)

En choisissant le _mode 2 joueurs_, vous pouvez rentrer le nom de chacun des joueurs. Il est à noter que le joueur qui met son nom en premier jouera peut-être en deuxième ! Si vous avez choisis le _mode 1 joueur_, une partie se lance contre l'_ordinateur_.

## Le déroulement d'une partie

![Choisir une piece](https://raw.githubusercontent.com/PictElm/Quartoto/master/images/game_choose_piece.png)

Le joueur qui ouvre la partie choisis la première pièce que l'autre devra placer. Dirigez-vous sur la pièce de votre choix avec les **flèches directionnels** **droite** et **gauche** (ou **haut** et **bas** selon votre préférence), puis appuyer sur la touche **entrer** pour continuer.

![Placer une piece](https://raw.githubusercontent.com/PictElm/Quartoto/master/images/game_place_piece.png)

Le joueur suivant doit alors placer la pièce en question sur le plateau où il lui convient. Encore une fois, il doit utiliser les **flèches directionnels** **haut**, **bas**, **droite** et **gauche** pour se déplacer, puis **entrer** pour placer la pièce.

## La fin de la partie

![Fin de partie](https://raw.githubusercontent.com/PictElm/Quartoto/master/images/game_end.png)

La partie se déroule ainsi, alternant les tours, jusqu'à ce qu'un joueur remarque qu'une ligne est complétée ! Il peut alors, juste après avoir placé sa pièce, signaler le **Quarto!** en sélectionnant l'option approprier **dans la liste des pièces**. Il remporte ainsi la partie. Vous pouvez ensuite décider de jouer une revanche : dans ce cas, le **joueur qui a perdu la partie commence**.

## L'option **Quarto!**

![Option dire quarto](https://raw.githubusercontent.com/PictElm/Quartoto/master/images/game_say_quarto.png)

L'option **Quarto!** apparait dès qu'il y a une pièce posée sur le plateau (donc dès la fin du premier tous). L'option peut donc être sélectionner lors de la partie du tour ou on choisit la pièce à donner à l'adversaire (juste après avoir posé soi-même). Note : un **Quarto!** non signaler reste valide pour le joueur suivant, mais pas plus ! (Comme indiquer dans les règles.)

## ATTENTION !!!

![Mauvaise fin](https://raw.githubusercontent.com/PictElm/Quartoto/master/images/game_end_bad.png)

Si vous pensez avoir vus un **Quarto!**, c'est mieux de revérifier avant de sélectionner l'option "Quarto" : si vous vous étiez trompé, vous perdez la partie et l'adversaire l'emporte !

---

## Cas du _mode 1 joueur_

![Difficultee](https://raw.githubusercontent.com/PictElm/Quartoto/master/images/game_difficulty.png)

Dans un jeu contre l'_ordinateur_, ce dernier adapte son niveau de difficulté à la fin de la partie. Au niveau de difficulté minimal (0%), l’ordinateur joue au hasard. Lorsqu'une partie est gagnée, la difficulté évolue en fonction du nombre de tours de la partie. Ainsi une partie gagnée en 4 coups (très rapide) fait rapidement varier la difficulté. Lorsque le seuil de difficulté atteint 100%, l'ordinateur ne fait plus aucune erreur!
