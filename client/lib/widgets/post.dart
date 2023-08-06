import 'package:flutter/material.dart';
import '../models/post.dart';
import '../configuration.dart';

class Post extends StatelessWidget {
  PostData data;

  Post(this.data, { super.key });

  @override
  Widget build(BuildContext context) {
    List<Widget> children = [];
    children.add(Text(data.description ?? ""));

    if (data.imageId != 'null') {
      children.add(const SizedBox(height: 10));
      children.add(Image.network('${Configuration.azureContainer}/${data.imageId}'));
    }

    return Container(
      decoration: const BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(color: Colors.grey, blurRadius: 5)
        ]
      ),
      padding: const EdgeInsets.all(10),
      margin: const EdgeInsets.all(5),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: children
      )
    );
  }
}