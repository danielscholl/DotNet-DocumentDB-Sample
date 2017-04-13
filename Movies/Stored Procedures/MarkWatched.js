function markWatched() {
    var collection = getContext().getCollection();
    var response = getContext().getResponse();

    collection.queryDocuments(
        collection.getSelfLink(),
        "SELECT * FROM movies m where m.watched = false",
        function (error, documents, responseOptions) {
            if (error) throw new Error(error.body);

            documents.forEach(function (document) {
                document.watched = true;

                collection.replaceDocument(
                    document._self, document,
                    function (error, resource, options) {
                        if (error) throw new Error(error.body);

                        response.setBody(resource);
                    });
            });
        });
};