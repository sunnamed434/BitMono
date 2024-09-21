# Use a base image with Python
FROM python:3.10-slim

# Set working directory
WORKDIR /docs

# Copy the documentation source into the container
COPY . /docs

# Install Sphinx and build the documentation
RUN pip install sphinx sphinx-rtd-theme
RUN sphinx-build -b html source build

# Expose the port the app runs on
EXPOSE 8000

# Command to run the HTTP server and output instructions
CMD echo "Documentation is available at http://localhost:8000" && \
    python -m http.server 8000 --directory build