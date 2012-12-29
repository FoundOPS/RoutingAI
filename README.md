# Routing AI #

## RoutingAI #
This is a dynamic library with pretty much everything in it. Especially data types shared between controller and slave. Framework code also goes here.

## RoutingAI.Controller ##
This is a console application frontend that starts a network server to expose RoutingAI controls.

## RoutingAI.Librarian ##
This is a concole application that does miscellaneous work. For example, it fulfills the role of data keeper during K-Means clustering of nodes.
**Note:** This application doesn't have to run on a dedicated machine, instead, it can be run on the same machine with controller. One reason for implementhing this as a separate application is because it may become memory hungry and therefore many instances might be needed.

## RoutingAI.Slave ##
This is a console application backend that starts a network server to schedule and process tasks assigned by RoutingAI.Controller instance.