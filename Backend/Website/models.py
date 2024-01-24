
from . import db
# Import from current package
from flask_login import UserMixin
from sqlalchemy.sql import func
# Module to help us log in users


# When creating a new database model, define the NAME of the object, INHERIT from DB.MODEL

class User(db.Model, UserMixin):
    id = db.Column(db.Integer, primary_key=True)
    email = db.Column(db.String(150), unique=True) # No user can have the same email
    password = db.Column(db.String(150))
    username = db.Column(db.String(150))
    notes = db.relationship('Note') # Tell Flask/Alchemy, for every note, add the user note relationship
    # MUST BE UPPERCASE
    # One to may relationship (user has many Notes)

class Note(db.Model):
    id = db.Column(db.Integer, primary_key=True)
    data = db.Column(db.String(10000))
    date = db.Column(db.DateTime(timezone=True), default=func.now()) # Automatically get surrent date and time
    user_id = db.Column(db.Integer, db.ForeignKey('user.id')) # lowercase is cool