# Anything not related to authentication that the user can navigate to
from flask import Blueprint, render_template, request, flash, redirect, url_for
from werkzeug.security import generate_password_hash, check_password_hash
from .models import User
from . import db
from flask_login import login_user, login_required, logout_user, current_user # Reason for UserMixin

# This file is a blueprint for our application (collection of URLs)

auth = Blueprint('auth', __name__)

@auth.route('/login', methods=['GET', 'POST'])
def login():
	if request.method == 'POST':
		email = request.form.get('email')
		password = request.form.get('password')
		
		# Search database
		user = User.query.filter_by(email=email).first()
		if (user):
			if check_password_hash(user.password, password):
				flash('Logged In Successfully', category='success')
				login_user(user, remember=True) # Session/cookies saver - while server is running
				return redirect(url_for('views.home'))
			else:
				flash('Password is incorrect', category='err')
		else:
			flash('User does not exist', category='err')
			
	return render_template("login.html", user=current_user) #"<p>Login</p>"


@auth.route('/logout')
@login_required # Cannot use this unless user is logged in
def logout():
	logout_user()
	return redirect(url_for("auth.home")) #"<p>Logout</p>"

@auth.route('/sign-up', methods=['GET', 'POST'])
def sign_up():
	if (request.method == 'POST'):
		email = request.form.get('email')
		username = request.form.get('username')
		password1 = request.form.get('password1')
		password2 = request.form.get('password2')
		
		# Valid POST check
		user = User.query.filter_by(email=email).first()
		if (user):
			flash('Email Exists', category='err')
		elif (password1 != password2):
			flash('Passwords do not match', category='err')
			pass
		
		elif (len(email) < 4):
			flash('Email is not long enough', category='err')
			pass
		
		elif (len(password1) < 7):
			flash('Password not long enough', category='err')
			pass
		else:
			new_user = User(email=email, username=username, password=generate_password_hash(password1, method='scrypt'))
			db.session.add(new_user)
			db.session.commit()
			flash('Account created!', category='success')
			# Add to database
			login_user(user, remember=True)
			return redirect(url_for('views.home'))
			
	return render_template("sign-up.html", user=current_user)#"<p>Sign Up</p>"

@auth.route('/home')
@login_required
def home():
	return render_template("home.html", user=current_user)