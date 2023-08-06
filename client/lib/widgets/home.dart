import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:http/http.dart' as http;
import 'package:http_parser/http_parser.dart';
import 'package:file_picker/file_picker.dart';
import 'package:mime/mime.dart';
import 'post.dart';
import 'login.dart';
import '../models/post.dart';
import '../configuration.dart';

class Home extends StatefulWidget {
  const Home({ super.key });

  @override
  State<Home> createState() => HomeState();
}

class HomeState extends State<Home> {
  List<PostData> posts = List<PostData>.empty(growable: true);
  final descriptionController = TextEditingController();
  File? file;
  bool loading = true;

  Future<void> getPosts() async {
    var client = http.Client();

    setState(() => loading = true);

    try {
      var token = await FirebaseAuth.instance.currentUser?.getIdToken();

      var res = await client.get(Uri.https(Configuration.api, "posts"), headers: {
        'Authorization': 'Bearer $token'
      });

      var json = jsonDecode(res.body) as Iterable;

      posts.clear();

      setState(() {
        for (var data in json) {
          posts.add(PostData.fromJson(data));
        }

        loading = false;

        posts = posts.reversed.toList();
      });
    }

    finally {
      client.close();
    }
  }

  @override
  void initState() {
    super.initState();

    FirebaseAuth.instance.authStateChanges().listen((user) {
      if (user == null) {
        Navigator.pushReplacement(
            context,
            MaterialPageRoute(builder: (context) => const Login())
        );
      }
    });

    getPosts();
  }

  @override
  void dispose() {
    super.dispose();
    descriptionController.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("Home"),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () {
              FirebaseAuth.instance.signOut();
            }
          )
        ]
      ),
      body:
        !loading ?
          ListView.builder(
            itemCount: posts.length,
            itemBuilder: (context, index) {
              final post = posts[index];

              return Post(post);
            }
          )
        : const CircularProgressIndicator(),
      floatingActionButton: FloatingActionButton(
        child: const Icon(Icons.add),
        onPressed: () {
          showDialog(
            context: context,
            builder: (dialogContext) {
              return Dialog(
                insetPadding: const EdgeInsets.all(10),
                child: Container(
                    padding: const EdgeInsets.all(10),
                    child: Column(
                    children: [
                      const Text("Make Post"),
                      TextFormField(
                        decoration: const InputDecoration(
                          labelText: "Description"
                        ),
                        controller: descriptionController
                      ),
                      IconButton(
                        icon: const Icon(Icons.upload_file),
                        onPressed: () async {
                          FilePickerResult? result = await FilePicker.platform.pickFiles();

                          if (result != null) {
                            setState(() {
                              file = File(result.files.single.path ?? "");
                            });
                          }
                        }
                      ),
                      TextButton(
                        child: const Text("Submit"),
                        onPressed: () async {
                          var uri = Uri.https(Configuration.api, 'posts/create');
                          var req = http.MultipartRequest("POST", uri);

                          req.fields["description"] = descriptionController.text;

                          if (file != null) {
                            var mimeType = lookupMimeType(file!.path);
                            var types = mimeType?.split('/');

                            if (types != null) {
                              req.files.add(await http.MultipartFile.fromPath(
                                'file',
                                file!.path,
                                contentType: MediaType(types[0], types[1])
                              ));
                            }
                          }

                          var token = await FirebaseAuth.instance.currentUser?.getIdToken();

                          req.headers["Authorization"] = 'Bearer $token';

                          await req.send();
                          await getPosts();

                          Navigator.pop(dialogContext);
                        }
                      )
                    ]
                  )
                )
              );
            }
          );
        }
      ),
    );
  }
}