How to Run BitMono Docs Locally

Prerequisites:
- Ensure Docker is installed on your machine. If not, install Docker from https://docs.docker.com/get-docker/.

Running Locally:
1. Start Docker if it's not already running.
2. Open a terminal and navigate to the directory containing your docker-compose.yml file.
3. Execute the following command to build and start the container:
   docker-compose up --build
4. Open your browser and go to: http://localhost:8000 to view the documentation.

Restarting the Container:
1. To stop the container, press CTRL + C in the terminal where Docker is running, or close the terminal window.
2. To restart the container and apply any changes, execute:
   docker-compose up --build
3. Refresh the browser page at http://localhost:8000 to see the latest updates.

To edit/update docs:
Prerequisites:
- Use Visual Studio Code (tps://code.visualstudio.com/), or any other tool.

Go to 'source' folder and do your magic!