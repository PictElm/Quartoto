# Quartoto

L’objectif de ce projet est la programmation du jeu de Quarto en C#.
Règles du jeu
Quarto est un jeu de société édité par la société [Gigamic](https://www.gigamic.com). L'objectif du jeu est d’aligner quatre pièces ayant au moins un point commun entre elles. Mais chaque joueur ne joue pas ce qu'il veut, c’est son adversaire qui choisit pour lui.

Vous trouverez plus d’informations sur la [page Wikipedia](https://fr.wikipedia.org/wiki/Quarto) et dans les [règles du jeu](https://www.gigamic.com/files/catalog/products/rules/quarto_rule-fr.pdf).
Fonctionnalités
Le programme réalisé permet à un joueur humain d’affronter l’ordinateur selon les règles standard du jeu.

---

## Tout d'abord, lancer le programme...

![Menu 2 Joueurs](https://raw.githubusercontent.com/PictElm/Quartoto/master/image/menu_2_players.png)

En choisissant le *mode 2 joueurs*, vous pouvez rentrer le nom de chacun des joueurs. Il est à noter que le joueur qui met son nom en premier jouera peut-être en deuxième ! Si vous avez choisis le *mode 1 joueur*, une partie se lance contre l'__ordinateur__.

## Le déroulement d'une partie

![Menu 2 Joueurs](https://raw.githubusercontent.com/PictElm/Quartoto/master/image/game_choose_piece.png)

Le joueur qui ouvre la partie choisis la première pièce que l'autre devra placer. Dirigez-vous sur la pièce de votre choix avec les *flèches directionnels* *droite* et *gauche* (ou *haut* et *bas* selon votre préférence), puis appuyer sur la touche *entrer* pour continuer.

![Menu 2 Joueurs](https://raw.githubusercontent.com/PictElm/Quartoto/master/image/game_place_piece.png)

Le joueur suivant doit alors placer la pièce en question sur le plateau où il lui convient. Encore une fois, il doit utiliser les *flèches directionnels* *haut*, *bas*, *droite* et *gauche* pour se déplacer, puis *entrer* pour placer la pièce.

## La fin de la partie

![Menu 2 Joueurs](https://raw.githubusercontent.com/PictElm/Quartoto/master/image/game_end.png)

La partie se déroule ainsi, alternant les tours, jusqu'à ce qu'un joueur remarque qu'une ligne est complétée ! Il peut alors, juste après avoir placé sa pièce, signaler le *Quarto!* en sélectionnant l'option approprier *dans la liste des pièces*. Il remporte ainsi la partie. Vous pouvez ensuite décider de jouer une revanche : dans ce cas, le *joueur qui a perdu la partie commence*.

### ATTENTION !!!

Si vous pensez avoir vus un *Quarto!*, c'est mieux de revérifier avant de sélectionner l'option "Quarto" : si vous vous étiez trompé, vous perdez la partie et l'adversaire l'emporte !

---

## Cas du *mode 1 joueur*

![Menu 2 Joueurs](https://raw.githubusercontent.com/PictElm/Quartoto/master/image/game_difficulty.png)

Dans un jeu contre l'__ordinateur__, ce dernier adapte son niveau de difficulté à la fin de la partie. Au niveau de difficulté minimal (0%), l’ordinateur joue au hasard. Lorsqu'une partie est gagnée, la difficulté évolue en fonction du nombre de tours de la partie. Ainsi une partie gagnée en 4 coups (très rapide) fait rapidement varier la difficulté. Lorsque le seuil de difficulté atteint 100%, l'ordinateur ne fait plus aucune erreur!
