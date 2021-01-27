image=paulopez/meetup-events:0.1
#build_path=./publish

#rm -r $build_path
#docker run -w /app -v $(pwd):/app mcr.microsoft.com/dotnet/sdk dotnet publish ./MeetupEvents -o $build_path

#docker build -t $image -f Dockerfile ./publish


docker build -t $image ./MeetupEvents
docker push $image
docker run -p 8080:5000 $image
