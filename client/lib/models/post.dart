class PostData {
  int? id;
  String? description;
  String? userId;
  String? imageId;

  static PostData fromJson(Map<dynamic, dynamic> data) {
    var post = PostData();
    post.id = int.parse(data["id"].toString());
    post.description = data["description"].toString();
    post.userId = data["userId"].toString();
    post.imageId = data["imageId"].toString();

    return post;
  }
}