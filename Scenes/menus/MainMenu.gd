extends Node

func _ready() -> void:
	pass

func _on_play_button_pressed() -> void:
	LoadManager.load_scene("res://Scenes/GameLoop/GameLoop.tscn")

func _on_quit_button_pressed() -> void:
	get_tree().quit();
