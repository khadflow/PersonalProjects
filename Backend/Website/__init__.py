from email.mime import application
from flask import Flask
from flask_sqlalchemy import SQLAlchemy
from os import path
from flask_login import LoginManager

# Tutorial : https://www.youtube.com/watch?v=dam0GPOAvVI&t=66s

# Database to ADD/STORE/DELETE Info
db = SQLAlchemy()
DB_NAME = "database.db"

# Create a Flask Application
def create_app():
	app = Flask(__name__)
	app.config['SECRET_KEY'] = '1234' #Cookies and Session data secret key

	# Databse initialize
	app.config['SQLALCHEMY_DATABASE_URI'] = f'sqlite:///{DB_NAME}'
	# This is the app we're going to use with this model
	db.init_app(app)


	# Managing Log In
	login_manager = LoginManager()
	login_manager.login_view = "auth.login"
	login_manager.init_app(app)

	@login_manager.user_loader
	def load_user(id):
		return User.query.get(int(id))

	# Check views
	from .views import views
	from .auth import auth

	app.register_blueprint(views, url_prefix='/')
	app.register_blueprint(auth, url_prefix='/')
	# url_prefix == what comes before the Blueprint route

	from .models import User, Note
	
	create_database(app)

	return app


def create_database(app):
	if not path.exists('Website/' + DB_NAME):
		with app.app_context():
			db.create_all()
		print('Created Database!')