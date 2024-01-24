# Anything not related to authentication that the user can navigate to
from unicodedata import category
from flask import Blueprint, render_template, request, flash, jsonify
from flask_login import login_user, login_required, logout_user, current_user # Reason for UserMixin
from .models import Note
from . import db
import json


# This file is a blueprint for our application (collection of URLs)

views = Blueprint('views', __name__)

# @nameOfBlueprint.route(URL to get to this endpoint) '/' == homepage/root
# Will call home()
@views.route('/', methods=['GET', 'POST'])
@login_required
def home():
	if (request.method == 'POST'):
		content = request.form.get("Note") # Always the name in the HTML attribute
		if (len(content) < 1):
			flash("Note content is empty.", category='err')
		else:
			new_note = Note(data=content, user_id=current_user.id) # current_user lets us access all of the fields of the User
			db.session.add(new_note)
			db.session.commit()
			flash("Successfully added the note", category="success")
		print(content)
		
	return render_template("home.html", user=current_user)


@views.route('/delete-note', methods=['POST'])
def delete_note():
	note = json.loads(request.data)
	noteID = note['noteId']
	note = Note.query.get(noteID)
	if note:
		if note.user_id == current_user.id:
			db.session.delete(note)
			db.session.commit()
			return jsonify({})
	return jsonify({})