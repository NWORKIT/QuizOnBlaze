# QuizOnBlaze
**QuizOnBlaze** is an Interactive quiz build in Blazor created by [Ricardo Cabral](https://www.rramoscabral.com) during his free time with the support of [NWORKIT Digital Solutions, Unipessoal Lda](https://www.nworkit.pt) for his technical session **Build Web Apps with Blazor: .NET for Lazy (But Productive) Devs** for the conference **KulenDayz — Slow down IT Conference** in Osijek, Croatia. 


## Build Web Apps with Blazor: .NET for Lazy (But Productive) Devs
- Discover the power of Blazor for building web applications using ASP.NET Core.
- This introductory level session will cover the basics of setting up a Blazor project, creating components, and managing state.
- Get ready to build your first Blazor web application!

<br/>

The goal is not to create a competitor to Kahoot but to demonstrate the features of Blazor in ASP.NET 9 in the technical session.

<br/>

**What is Blazor?**

Blazor is a modern front-end web framework based on HTML, CSS, and C# that helps you build web apps faster. With Blazor, build web apps using reusable components that can be run from both the client and the server so that you can deliver great web experiences.

<br/>


## Notes
- Some components use events and parameters to demonstrate data pass-through
- SignalR receives and sends messages to all clients, administration, and individual clients to demonstrate the same.
- I chose not to create any dependency injection (DI) services for SignalR.
- There may be comments written in Portuguese.


<br/>

## Question file format

The questions are uploaded via a JSON (JavaScript Object Notation) file with the following structure

```json
[
  {
    "QuestionText": "Question text",
    "QuestionImage": "URL of the image related to the question",
    "QuestionOptions": [ "Answer Option 0", "Answer Option 1", "Answer Option 3"],
    "QuestionCorrectAnswer": 0
  }
]
```

Example


```json
[
  {
    "QuestionText": "What is the capital of Portugal?",
    "QuestionImage": "https://upload.wikimedia.org/wikipedia/commons/thumb/4/48/Portugal_location_map.svg/250px-Portugal_location_map.svg.png",
    "QuestionOptions": [ "Lisboa", "Porto", "Faro", "Braga" ],
    "QuestionCorrectAnswer": 0
  },
  {
    "QuestionText": "How much is 5 x 5?",
    "QuestionImage": "https://estudoemcasaapoia.dge.mec.pt/sites/default/files/h5p/content/793/images/file-629f47ddba9be.jpg",
    "QuestionOptions": [ "30", "11", "56", "25" ],
    "QuestionCorrectAnswer": 3
  },
  {
    "QuestionText": "Who wrote 'The Lusiads'?",
    "QuestionImage": "https://upload.wikimedia.org/wikipedia/commons/0/0d/Os_Lus%C3%ADadas.jpg",
    "QuestionOptions": [ "Pessoa", "Camões", "Saramago", "Eça de Queirós" ],
    "QuestionCorrectAnswer": 1
  }
]
```


<br/>

---

[⭐️ Star our repo](https://github.com/NWORKIT/QuizOnBlaze)


<br/>

## License

This project is licensed under the MIT license for non-commercial use.

For commercial use, please contact [comercial@nworkit.pt](malito:comercial@nworkit.pt?subject=QuizOnBlaze) for permission.
