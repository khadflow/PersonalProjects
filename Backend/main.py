from Website import create_app
# __init__ makes Website a Python Package, we can import anything in the __init__ file

app = create_app()

# Only run we server if we run this specific file
if __name__ == '__main__':
	# Changes to Python files will automatically rerun the web server
	app.run(debug=True)


# RUN SERVER LOCALLY : python main.py